namespace Base.Enums
{
    /// <summary>
    /// query item type
    /// </summary>
    public enum QitemTypeEnum
    {
        //Identity,
        //Identity2,  //child table maps to parent Identity
        None,
        //Num,
        Date,       //if only input one date then find this day, if date of xxx2 existed then find range
        Date2,      //2nd date field(start/end), for query
        DT,         //datetime
        //TimeTick,
        //Html,
    }
}
