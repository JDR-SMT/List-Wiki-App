using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListWikiApp
{
    public class Information : IComparable<Information>
    {
        // private variables
        private string? name;
        private string? category;
        private string? structure;
        private string? definition;

        #region Getters & Setters
        public string GetName()
        {
            return name;
        }

        public void SetName(string aName)
        {
            name = aName;
        }

        public string GetCategory()
        {
            return category;
        }

        public void SetCategory(string aCategory)
        {
            category = aCategory;
        }

        public string GetStructure()
        {
            return structure;
        }

        public void SetStructure(string aStructure)
        {
            structure = aStructure;
        }

        public string GetDefinition()
        {
            return definition;
        }

        public void SetDefinition(string aDefinition)
        {
            definition = aDefinition;
        }
        #endregion

        // uses IComparable to sort by name
        public int CompareTo(Information other)
        {
            return name.CompareTo(other.name);
        }
    }
}
