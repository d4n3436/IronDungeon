namespace IronDungeon.API
{
    public enum ActionType
    {
        Progress,
        Continue,
        Undo,
        Redo,
        Alter,
        Remember
    }

    public enum InputType
    {
        Do,
        Say,
        Story,
        None
    }
}