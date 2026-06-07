
namespace Base.Enums
{
    /// <summary>
    /// crud fun type, 順序同為權限的子功能順序
    /// 邏輯判斷使用正向表列
    /// </summary>
    public enum CrudEnum
    {
        AuthRow,    //pos 0, fun is auth row or not(0/1)
        Create,     //0/1
        Read,
        Update,
        Delete,
        Print,
        Export,
        View,
        Other,
        //Sign,       //簽核(在other後面，不出現在權限設定畫面), 必須要有新增權限!!
        Empty = 99
    }
}
