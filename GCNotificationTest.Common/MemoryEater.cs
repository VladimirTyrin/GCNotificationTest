using System.Collections.Generic;
using System.Linq;

namespace GCNotificationTest.Common
{
    public static class MemoryEater
    {
        public static long AllocateSomeObjects(int weight, bool cpuIntensive)
        {
            var objectCount = weight * 1000;
            var unusedResult = 0L;

            for (var i = 0; i < objectCount; ++i)
            {
                var instance = new SomeClass(i, cpuIntensive);
                unusedResult ^= instance.GetHashCode();
            }

            return unusedResult;
        }

        public class SomeClass
        {
            public SomeStruct SomeStruct { get; }
            public LinkedList<SomeStruct> List { get; } = new LinkedList<SomeStruct>();
            public readonly byte[] Bytes;

            private readonly bool _cpuIntensive;

            public SomeClass(int x, bool cpuIntensive)
            {
                SomeStruct = new SomeStruct(x, x + 1);
                _cpuIntensive = cpuIntensive;

                for (var i = 0; i < 1000; ++i)
                {
                    List.AddLast(new LinkedListNode<SomeStruct>(new SomeStruct(i, i + 1)));
                }

                Bytes = new byte[1020];
            }

            public override int GetHashCode()
            {
                var result = SomeStruct.GetHashCode();
                if (!_cpuIntensive)
                    return result;

                return result ^ List.Aggregate(0, (curr, str) => curr ^ str.GetHashCode());
            }
        }

        public struct SomeStruct
        {
            public readonly int A;
            public readonly int B;
            public readonly byte[] Bytes;

            public SomeStruct(int a, int b)
            {
                A = a;
                B = b;
                Bytes = new byte[1000];
            }

            public override int GetHashCode()
            {
                return A ^ B;
            }
        }
    }
}
