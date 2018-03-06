using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using SerilogWeb.Classic;

[assembly: AssemblyTitle("Serilog.Web")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCopyright("Copyright © Serilog Contributors 2018")]

[assembly: PreApplicationStartMethod(typeof(ApplicationLifecycleModule), nameof(ApplicationLifecycleModule.Register))]

[assembly: InternalsVisibleTo("SerilogWeb.Classic.Tests, PublicKey=" +
"00240000048000009400000006020000002400005253413100040000010001008bae550e3b8cfe" +
"075acd1d1a6084a6dbb84d1acd68d1c7ffe320032ea9ae4f5b91e37c61861e405481a293063ea8" +
"1f285450282b41728b2aa23c66fa41a207bcbbea1ec987ac5b4cc53125cc18f9c7c6b891c31d05" +
"968dfb74b186dc2a5b715b8e6280a1c7e20bf8ae4a39f2225d2b1eca421345b145e6f0c2224cd6" +
"fefff0aa")]