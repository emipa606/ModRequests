namespace Revolus.TextileStats {
    public struct ColumnLabelHeader {
        public readonly string optionalLabel;
        public readonly int columnSpan;

        public ColumnLabelHeader(string optionalLabel, int columnSpan) {
            this.optionalLabel = optionalLabel;
            this.columnSpan = columnSpan;
        }
    }
}
