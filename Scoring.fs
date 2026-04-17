module Scoring

open System
open Domain

let calculateScore (now: DateTime) (task: Task) : float =
    let deadlineWeight =
        match task.Deadline with
        | None -> 0.0
        | Some d ->
            let hoursLeft = (d - now).TotalHours
            if   hoursLeft <= 0.0   then 100.0
            elif hoursLeft <= 24.0  then 80.0
            elif hoursLeft <= 72.0  then 50.0
            elif hoursLeft <= 168.0 then 25.0
            else 5.0

    let importanceScore  = float task.Importance * 10.0
    let difficultyPenalty = float task.Difficulty * 3.0

    let energyCost =
        match task.EnergyReq with
        | Light    -> 0.0
        | Moderate -> 5.0
        | Heavy    -> 12.0

    deadlineWeight + importanceScore - difficultyPenalty - energyCost

let defaultMessages : ReasonMessages = {
    DeadlinePassed   = "Deadline passed!"
    DeadlineUnder24h = "Deadline < 24h"
    DeadlineUnder3d  = "Deadline within 3 days"
    NoDeadline       = "No deadline"
    HighImportance   = "High importance"
    DifficultTask    = "Difficult task"
    HighEnergy       = "High energy required"
}

let generateReason (now: DateTime) (msgs: ReasonMessages) (task: Task) : string =
    let reasons = ResizeArray<string>()

    match task.Deadline with
    | Some d when (d - now).TotalHours <= 0.0  -> reasons.Add(msgs.DeadlinePassed)
    | Some d when (d - now).TotalHours <= 24.0 -> reasons.Add(msgs.DeadlineUnder24h)
    | Some d when (d - now).TotalHours <= 72.0 -> reasons.Add(msgs.DeadlineUnder3d)
    | None                                      -> reasons.Add(msgs.NoDeadline)
    | _                                         -> ()

    if task.Importance >= 4 then reasons.Add(msgs.HighImportance)
    if task.Difficulty  >= 4 then reasons.Add(msgs.DifficultTask)

    match task.EnergyReq with
    | Heavy -> reasons.Add(msgs.HighEnergy)
    | _     -> ()

    String.concat " · " reasons

let getTopTask (msgs: ReasonMessages) (tasks: Task list) : ScoredTask option =
    let now = DateTime.Now
    tasks
    |> List.filter (fun t -> not t.Done)
    |> List.map (fun t ->
        { Task        = t
          Score       = calculateScore now t
          Reason      = generateReason now msgs t
          BurnoutFlag = false })
    |> List.sortByDescending (fun st -> st.Score)
    |> List.tryHead

let detectBurnout (tasks: Task list) : bool =
    tasks
    |> List.filter (fun t -> not t.Done)
    |> List.filter (fun t -> t.EnergyReq = Heavy || t.Difficulty >= 4)
    |> List.length
    |> fun count -> count >= 3
