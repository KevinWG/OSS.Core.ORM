using OSS.Common.BasicMos;

namespace OSS.Core.ORM.Tests
{
    public class UserInfo : BaseMo<long>
    {
        public string name { get; set; }
    }


    public class UserBigInfo : UserInfo
    {
        public int age { get; set; }
    }
}