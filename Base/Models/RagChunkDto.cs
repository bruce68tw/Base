namespace Base.Models
{
    //RAG chunk, 配合來源json檔案, 使用短欄位名
    public class RagChunkDto
    {
        //question
        public string Q { get; set; } = "";

        //answer
        public string A { get; set; } = "";

        //is chunk or not
        public bool C { get; set; } = false;
    }

}