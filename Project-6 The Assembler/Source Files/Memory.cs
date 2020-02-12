namespace Project6
{
    public class Memory
    {
        private const int BASE = 16;

        private int space = BASE;

        public int Allocate() => space++;
    }
}