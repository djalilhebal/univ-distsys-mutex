# TODO (Meikodayo)

- [ ] **Important: Review all algorithm implementations!**
    * TLDR: Fact check everything and properly cite algorithms!
    * Most of the implemented algorithms are either inaccurately or vaguely taught to us (sometimes described as prose, omitted the algorithm pseudo-code, or otherwise contain typos) and no precise/direct citations/references were provided (so I couldn't find the original algorithm and compare it with my implementation, like what is "Chandy-Misra"? All I could find is "Chandy-Misra-Hass Detection Algorithm", which is different from what we "learned").

- [ ] `AbstractAlgo`: Use `Acquire()` and `Release()` instead of `DoEnter()` and `DoLeave()`.
(More common names, as in `Semaphore::Acquire` and `Semaphore::Release`.)

- [ ] `AbstractAlgo`: Add `void` methods: `OnMessage(MeikoMessage)`.
```diff
while (! cond )
{
-    /* no-op */;
+    // Handle the next incoming message. Maybe it will affect `cond`.
+    OnMessage( ReceiveMessage() );
}
```

- [ ] Node ids should be `int`, not `char`.
    * I've initially chosen `char` because I was lazy and wanted to be easily print them.

- [ ] Predefine message types for each algorithm, in a List (e.g. `IReadOnlyList<T>`) or an enum (but still convert values `ToString()`).
```cs
public static readonly IList<string> MessageTypes = ...;
```

- [ ] Maybe use this C# wrapper instead of calling `plantuml.jar` "manually" from the command line  
https://github.com/KevReed/PlantUml.Net

- [ ] Properly find and call the SiteProcess. 
Either passed as a CLI argument (manually) or somehow automatically...  
Maybe useful: [`Process.GetCurrentProcess` | MSDN](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.getcurrentprocess?view=net-5.0).

---

END.
