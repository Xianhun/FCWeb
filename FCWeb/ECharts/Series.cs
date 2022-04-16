using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAWeb.Echarts
{
    public class Series
    {
        private object name;
        private object type;
        private List<object> data;

        public Series(object name, object type, List<object> data)
        {
            this.name = name;
            this.type = type;
            this.data = data;
        }

        public object getName()
        {
            return name;
        }
        public void setName(object name)
        {
            this.name = name;
        }
        public object getType()
        {
            return type;
        }
        public void setType(object type)
        {
            this.type = type;
        }
        public List<object> getData()
        {
            return data;
        }
        public void setData(List<object> data)
        {
            this.data = data;
        }       
        public object toobject()
        {
            return "Series [name=" + name + ", type=" + type + ", data=" + data.ToString() + "]";
        }

    }
}