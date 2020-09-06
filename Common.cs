using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureDemo.Functions
{
   public class Common
    {
    }
    public class TodoItem : TableEntity
    {
        public string Name { get; set; }


    }

    public class Todo
    {
        public string Name { get; set; }

        public static implicit operator TodoItem(Todo todo) => new TodoItem { RowKey = Guid.NewGuid().ToString(), PartitionKey = "demo", Name = todo.Name };
    }

    public class Model
    {
        public Todo value { get; set; }

        public static implicit operator TodoItem(Model model) => new TodoItem { RowKey = Guid.NewGuid().ToString(), PartitionKey = "demo", Name = model.value.Name };
    }
}
