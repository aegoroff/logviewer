// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 31.01.2015
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.Text;

namespace logviewer.engine.grammar
{
    internal class Composer : List<IPattern>, IPattern
    {
        public string Compose(IList<Semantic> messageSchema)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < this.Count; i++)
            {
                stringBuilder.Append(this[i].Compose(messageSchema));
            }

            return stringBuilder.ToString();
        }
    }
}
