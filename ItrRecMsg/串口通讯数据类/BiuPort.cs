using System;
using System.Collections.Generic;

using System.Text;
using System.Threading;

namespace ItrRecMsg
{
    /// <summary>
    /// 通讯接口方式
    /// </summary>
    public abstract class BiuPort : IDisposable
    {
        public abstract void Dispose();
    }
}