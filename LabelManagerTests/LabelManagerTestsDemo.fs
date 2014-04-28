module LabelManagerTestsDemo

open NUnit.Framework
open LabelManager
open System.Collections.Generic

[<Test>]
let ``newly created LabelManager should be empty``() = 
   let cm = LabelManager()
   Assert.IsEmpty(cm.Labels())

[<Test>]
let ``trying to get a Label by id when there is a Label of that index returns the correct name``() = 
   let cm = LabelManager()
   let expected = "Label4"
   for id in 0..9 do
      cm.Add(sprintf "Label%i" id) |> ignore
   let actual = cm.TryGetById(4)
   Assert.AreEqual(expected, actual)

// Concurrency tests:
[<Test>]
let ``elements can be added and retrieved by id concurrently``() = 
   let parallelCount = 1024
   let cm = LabelManager()
   let expected = true
   
   let actual = 
      try 
         [| 0..parallelCount - 1 |] 
         |> Array.Parallel.iter (fun i -> 
            let index = i / 2 + 1
            if i % 2 = 0 then 
               let name = sprintf "Label%i" index
               cm.Add(name) |> ignore
            else cm.TryGetById index |> ignore)
         true
      with e -> 
         printfn "Error: %s" e.InnerException.Message
         false
   Assert.AreEqual(expected, actual)

[<Test>]
let ``the columns property can be read concurrently with adding columns ``() = 
   let parallelCount = 1024
   let cm = LabelManager()
   let expected = true
   
   let actual = 
      try 
         [| 0..parallelCount - 1 |] 
         |> Array.Parallel.iter (fun i -> 
         let index = i / 2 + 1
         let name = sprintf "Column%i" index
         if 
            i % 2 = 0 then cm.Add(name) |> ignore
         else 
            cm.Labels() |> Seq.length |> ignore)
         true 
      with e -> 
         printfn "Error: %s" e.InnerException.Message
         false
   Assert.AreEqual(expected, actual)