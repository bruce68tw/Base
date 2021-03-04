namespace BaseWeb.Services
{
    //由系統自行決定如何得到用戶語系
    //在Base project會檢查是否有用DI註冊, 所以放在Base
    public interface ILocale
    {
        //get locale code, ex:zh-TW
        string GetLocale();

        void SetLocale(string locale);
    }
}
