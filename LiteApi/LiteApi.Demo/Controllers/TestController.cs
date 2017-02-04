﻿using System.Collections.Generic;
using System.Linq;

namespace LiteApi.Demo.Controllers
{
    public class TestController : LiteController
    {
        readonly IDemoService _service;

        public TestController(IDemoService service)
        {
            _service = service;
        }

        public int Add(int a, int b)
        {
            return _service.Add(a, b);
        }

        public string Add(string a, string b)
        {
            return _service.Add(a, b);
        }

        public int SumInts(int[] ints)
        {
            return ints.Sum();
        }

        // will never be called, always DoIt(int[] i) will be called
        public int Action1(int i)
        {
            return i;
        }

        public int Action1(int[] i)
        {
            return i.Sum();
        }

        // will never be called, always DoIt(int? i) will be called
        public int Action2(int i)
        {
            return i;
        }

        public int Action2(int? i)
        {
            return i ?? 0;
        }

        public int Action2(int[] i)
        {
            return i.Sum();
        }

        public int Action2(int?[] i)
        {
            return i.Select(x => x ?? -0).Sum();
        }

        public object SumNotNullable(List<int?> ints)
        {
            bool hasNulls = ints.Any(x => !x.HasValue);
            int sum = ints.Where(x => x.HasValue).Select(x => x.Value).Sum();
            return new
            {
                hasNulls, sum
            };
        }
    }
}
