module LabelManagerTests

open NUnit.Framework
open LabelManager
open System.Collections.Generic

[<Test>]
let ``newly created LabelManager should be empty``() = 
   let cm = LabelManager()
   Assert.IsEmpty(cm.Labels())

[<Test>]
let ``can add a column to LabelManager``() = 
   let cm = LabelManager()
   let expected = 0
   let actual = cm.Add("Label1")
   Assert.AreEqual(expected, actual)

[<Test>]
let ``adding Labels returns the correct indices``() = 
   let cm = LabelManager()
   let expected = [| 0..9 |]
   
   let actual = 
      [| for id in 0..9 do
            yield cm.Add(sprintf "Label%i" id) |]
   Assert.AreEqual(expected, actual)

[<Test>]
let ``adding Labels returns creates the correct Labels``() = 
   let cm = LabelManager()
   
   let expected = 
      [| for id in 0..9 do
            yield KeyValuePair((sprintf "Label%i" id), id) |]
   for id in 0..9 do
      cm.Add(sprintf "Label%i" id) |> ignore
   let actual = cm.Labels() |> Array.ofSeq
   Assert.AreEqual(expected, actual)

[<Test>]
let ``adding a duplicate Label returns the same index``() = 
   let cm = LabelManager()
   let ind1 = cm.Add("Label1")
   let ind2 = cm.Add("Label1")
   Assert.AreEqual(ind1, ind2)

[<Test>]
let ``trying to get a Label by name when there are no Labels returns null``() = 
   let cm = LabelManager()
   let expected = null
   let actual = cm.TryGetByName("Label1")
   Assert.AreEqual(expected, actual)

[<Test>]
let ``trying to get a Label by name when there are Labels but none of that name returns null``() = 
   let cm = LabelManager()
   for id in 0..9 do
      cm.Add(sprintf "Label%i" id) |> ignore
   let expected = null
   let actual = cm.TryGetByName("Label99")
   Assert.AreEqual(expected, actual)

[<Test>]
let ``trying to get a Label by name when there is a matching Label returns the correct index``() = 
   let cm = LabelManager()
   let expected = 4
   for id in 0..9 do
      cm.Add(sprintf "Label%i" id) |> ignore
   let actual = cm.TryGetByName("Label4")
   Assert.AreEqual(expected, actual)

[<Test>]
let ``trying to get a Label by id when there are no Labels returns None``() = 
   let cm = LabelManager()
   let expected = null
   let actual = cm.TryGetById(1)
   Assert.AreEqual(expected, actual)

[<Test>]
let ``trying to get a Label by id when there are Labels but none of that index returns null``() = 
   let cm = LabelManager()
   for id in 0..9 do
      cm.Add(sprintf "Label%i" id) |> ignore
   let expected = null
   let actual = cm.TryGetById(11)
   Assert.AreEqual(expected, actual)

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
let ``elements can be added and retrieved by name concurrently``() = 
   let parallelCount = 1024
   let cm = LabelManager()
   let expected = true
   
   let actual = 
      try 
         [| 0..parallelCount - 1 |] 
         |> Array.Parallel.iter (fun i -> 
         let index = i / 2 + 1
         let name = sprintf "Column%i" index
         if i % 2 = 0 then
            cm.Add(name) |> ignore
         else 
            cm.TryGetByName name |> ignore)
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

// Concurrency tests (DRY):
module Utils = 
   let TestParallel count (f : int -> unit) = 
      let expected = true
      
      let actual = 
         try 
            [| 0..count - 1 |] 
            |> Array.Parallel.iter (fun i -> (f i)) 
            true
         with e -> 
            printfn "Error: %s" e.InnerException.Message
            false
      Assert.AreEqual(expected, actual)

[<Test>]
let ``the columns property can be read concurrently with adding columns - DRY version``() = 
   let parallelCount = 1024
   let cm = LabelManager()
   
   let f i = 
      let index = i / 2 + 1
      let name = sprintf "Column%i" index
      if i % 2 = 0 then cm.Add(name) |> ignore
      else 
         cm.Labels()
         |> Seq.length
         |> ignore
   Utils.TestParallel parallelCount f

// Concurrency tests (SUPERDRY):
module Utils2 = 

   let rec private InmostException (e : System.Exception) =
      if e.InnerException = null then
         e
      else
         InmostException e.InnerException

   let TestParallel count (fs : array<int -> unit>) = 
      let expected = true
      let x = fs.Length
      
      let actual = 
         try 
            [| 0..count - 1 |] 
            |> Array.Parallel.iter (fun i -> 
               fs
               |> Array.Parallel.iter (fun f -> f i))
            true
         with e -> 
            printfn "Error: %s" ((InmostException e).Message)
            false
      Assert.AreEqual(expected, actual)

[<Test>]
let ``the columns property can be read concurrently with adding columns - SUPERDRY version``() = 
   let parallelCount = 1024
   let cm = LabelManager()
   
   let fAdd i = 
      let name = sprintf "Column%i" i
      cm.Add(name) |> ignore

   let fLabels i =
      cm.Labels()
      |> Seq.length
      |> ignore
   
   Utils2.TestParallel parallelCount [|fAdd; fLabels|]

[<Test>]
let ``the columns property can be read concurrently with adding and getting columns``() = 
   let parallelCount = 1024
   let cm = LabelManager()
   
   let fAdd i = 
      let name = sprintf "Column%i" i
      cm.Add(name) |> ignore

   let fLabels i =
      cm.Labels()
      |> Seq.length
      |> ignore
   
   let fTryGetByName i =
      let name = sprintf "Column%i" i
      cm.TryGetByName name |> ignore

   Utils2.TestParallel parallelCount [|fAdd; fLabels; fTryGetByName|]



