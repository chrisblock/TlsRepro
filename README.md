# TlsRepro

TLDR: This repository is a reproduction of a strange issue involving the `GenerateTargetFrameworkMonikerAttribute` MSBuild target that causes TLS 1.1+ to not work in .NET 4.6+.

It started as an issue I ran across at work, where some of our applications would get TLS errors when attempting to hit the HTTPS endpoints of our APIs. We had disabled TLS1.0 and lower, and were running .NET 4.7.2. All of the documentation I could find indicated that this should work.

I began by creating SDK projects for most every framework version (4.5, 4.6, 4.6.2, 4.7, 4.7.2, 4.8) in order to try to figure out what was going on. As the documentation described, all of these worked.

Next I tried a non-SDK 4.7.2 project (as most of our projects are this way), and once again, it worked.

I then began pulling in all the nugets our projects referenced in an attempt to see if that did anything. This, too, worked.

Then I compared the project files from my working test project and not-working original project and started making them the same. Eventually, I found that the test project did not contain a call to the `GenerateTargetFrameworkMonikerAttribute` MSBuild target. Upon adding this, my formerly working test project began to fail.

I am uncertain exactly what `GenerateTargetFrameworkMonikerAttribute` is supposed to do, but what it *does* do, is cause a `System.Runtime.Versioning.TargetFrameworkAttribute` to ***not*** be emitted on the generated assembly, which seems to affect (deep, deep in the guts) the workings of the `System.Net.ServicePointManager` and `System.AppContext` classes, in that they now behave as if you are running .NET 4.5, at least as far as SSL and TLS are concerned.

This can clearly be seen by looking at the MSIL from the included MANIFEST files.

```
.custom instance void [mscorlib]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 1C 2E 4E 45 54 46 72 61 6D 65 77 6F 72 6B   // ....NETFramework
                                                                                                      2C 56 65 72 73 69 6F 6E 3D 76 34 2E 37 2E 32 01   // ,Version=v4.7.2.
                                                                                                      00 54 0E 14 46 72 61 6D 65 77 6F 72 6B 44 69 73   // .T..FrameworkDis
                                                                                                      70 6C 61 79 4E 61 6D 65 14 2E 4E 45 54 20 46 72   // playName..NET Fr
                                                                                                      61 6D 65 77 6F 72 6B 20 34 2E 37 2E 32 )          // amework 4.7.2
```

This assembly level attribute is clearly missing from the TlsRepro.Broken.exe assembly.

Attempting to add the assembly level attribute manually causes an exception complaining that the string in the attribute is not of the correct length (I interpreted the hex from the MSIL as UTF-8, which is probably wrong).
