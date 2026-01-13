namespace EHub.Students;

public enum RelationShipToStudent
{
    Unknown = 0,

    // Parents/Guardians
    Father = 1,
    Mother = 2,
    Guardian = 3,
    StepFather = 4,
    StepMother = 5,

    // Siblings
    Brother = 6,
    Sister = 7,

    // Extended family
    Uncle = 8,
    Aunt = 9,
    Grandfather = 10,
    Grandmother = 11,

    // Other
    Self = 12,   // student themselves (for self-contact)
    Other = 13
}
