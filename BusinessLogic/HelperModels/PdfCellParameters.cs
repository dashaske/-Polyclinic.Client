﻿using System;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.HelperModels
{
    public class PdfCellParameters
    {
        public Cell Cell { get; set; }
        public string Text { get; set; }
        public string Style { get; set; }
        public ParagraphAlignment ParagraphAlignment { get; set; }
        public Unit BorderWidth { get; set; }
    }
}
