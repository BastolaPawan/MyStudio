namespace MyStudio.Controllers
{
    public class TestController
    {
        string _myname;
        public TestController(string myname)
        {
            _myname = myname;
        }


        public float getData(int x, int y)
        {
            return (float)x + y;
        }
    }
}
