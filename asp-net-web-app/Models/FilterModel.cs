public class FilterModel {

    public class FilterDefinition {
        private string[] acceptableTypes = ["Dropdown", "StartDate", "EndDate", "Search"];
        private string type;
        private string name;
        private list<T> data;

        public FilterDefinition(string type, dbSet<T> table){
            this.name = //tableName;
            if (acceptableTypes.contains(type)){this.type = type;}
            this.data = //tableData;//obviously only works with very small data set which is OK for this assignment
        }
    }
}

