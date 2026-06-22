namespace NICK2._0_core.Domain;

public enum EType
{
    Unknown = 0,
    Moving = 1,
    Packing = 3,
    MovingAndPacking = 4,
    LoadOnly = 5,
    UnloadOnly = 6,
    Commercial = 7,
    StorageInBound = 8,
    StorageOutBound = 9,
    InnerHouse = 10,
    JunkRemoval = 11,
    LaborOnly = 12
}
