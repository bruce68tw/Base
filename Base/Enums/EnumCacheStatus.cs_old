
namespace Base.Enums
{
    /// <summary>
    /// 查詢cache結果的狀態
    /// </summary>
    public enum EnumCacheStatus
    {
        Ok,         //成功, 包含資料內容是null的情形(與找不到資料不同)
        //OkButNull,  //成功, 但是資料內容是null
        Miss,       //資料有遺失(因資料異動被刪除)
        Empty,      //查無資料
        Error,      //系統錯誤(cache程式會記錄error)
    }
}
