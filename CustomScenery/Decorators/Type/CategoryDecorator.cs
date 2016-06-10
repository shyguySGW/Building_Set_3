using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Custom_Scenery.Decorators;
using UnityEngine;

namespace Custom_Scenery.CustomScenery.Decorators.Type
{
    class CategoryDecorator : IDecorator
    {
        public void Decorate(GameObject go, Dictionary<string, object> options, AssetBundle assetBundle)
        {
            go.GetComponent<BuildableObject>().categoryTag = (string)options["category"];
        }
    }
}
