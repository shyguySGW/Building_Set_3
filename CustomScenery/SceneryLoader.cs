using System;
using System.Collections.Generic;
using System.IO;
using Custom_Scenery.Decorators;
using MiniJSON;
using UnityEngine;

namespace Custom_Scenery
{
    internal class SceneryLoader : MonoBehaviour
    {
        private List<BuildableObject> _sceneryObjects = new List<BuildableObject>();

        public string Path;

        public string Identifier;

        public void LoadScenery()
        {
            try
            {
                var dict = Json.Deserialize(File.ReadAllText(Path + @"/scenery.json")) as Dictionary<string, object>;

                GameObject hider = new GameObject();

                char dsc = System.IO.Path.DirectorySeparatorChar;

                using (WWW www = new WWW("file://" + Path + dsc + "assetbundle" + dsc + "scenery"))
                {
                    if (www.error != null)
                        throw new Exception("Loading had an error:" + www.error);

                    AssetBundle bundle = www.assetBundle;

                    foreach (KeyValuePair<string, object> pair in dict)
                    {
                        try
                        {
                            var options = pair.Value as Dictionary<string, object>;
                            
                            GameObject asset = (new TypeDecorator((string)options["type"])).Decorate(options, bundle);
                            (new PriceDecorator((double)options["price"])).Decorate(asset, options, bundle);
                            (new NameDecorator(pair.Key)).Decorate(asset, options, bundle);

                            if (options.ContainsKey("grid"))
                                (new GridDecorator((bool)options["grid"])).Decorate(asset, options, bundle);
                            
                            if (options.ContainsKey("recolorable"))
                                (new RecolorableDecorator((bool)options["recolorable"])).Decorate(asset, options, bundle);

                            DontDestroyOnLoad(asset);

                            BuildableObject buildableObject = asset.GetComponent<BuildableObject>();
                            buildableObject.dontSerialize = true;
                            buildableObject.isPreview = true;

                            AssetManager.Instance.registerObject(buildableObject);
                            _sceneryObjects.Add(asset.GetComponent<BuildableObject>());

                            if (pair.Key == "Support Killer" || pair.Key == "Support Killer/Tiny")
                            {
                                AddBoundingBox(asset, 0.3f, 0.3f);
                            }
                        
                            // hide it from view
                            asset.transform.parent = hider.transform;
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);

                            LogException(e);
                        }
                    }

                    bundle.Unload(false);
                }

                hider.SetActive(false);
            }
            catch(Exception e)
            {
                LogException(e);
            }
        }

        public void AddBoundingBox(GameObject asset, float x, float z)
        {
            BoundingBox bb = asset.AddComponent<BoundingBox>();
            bb.isStatic = false;
            bb.layers = BoundingVolume.Layers.Buildvolume;
            Bounds b = new Bounds();
            b.center = new Vector3(0, 1, 0);
            b.size = new Vector3(x - .01f, .01f, z - .01f);
            bb.setBounds(b);
            bb.isStatic = true;
        }

        private void LogException(Exception e)
        {
            StreamWriter sw = File.AppendText(Path + @"/mod.log");

            sw.WriteLine(e);

            sw.Flush();

            sw.Close();
        }

        public void UnloadScenery()
        {
            foreach (BuildableObject deco in _sceneryObjects)
            {
                AssetManager.Instance.unregisterObject(deco);
            }
        }
    }
}
