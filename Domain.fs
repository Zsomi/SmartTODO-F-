module Domain

open System

type EnergyLevel =
    | Light
    | Moderate
    | Heavy

type Task = {
    Id          : Guid
    Title       : string
    Deadline    : DateTime option
    Difficulty  : int
    EnergyReq   : EnergyLevel
    Importance  : int
    CreatedAt   : DateTime
    Done        : bool
}

type ScoredTask = {
    Task        : Task
    Score       : float
    Reason      : string
    BurnoutFlag : bool
}
