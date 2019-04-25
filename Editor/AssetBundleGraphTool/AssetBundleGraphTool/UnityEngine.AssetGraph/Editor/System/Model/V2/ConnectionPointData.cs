using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.AssetGraph;
using V1 = AssetBundleGraph;

namespace UnityEngine.AssetGraph.DataModel.Version2
{
    [Serializable]
    public class ConnectionPointData
    {
        /**
        * In order to support Unity serialization for Undo, cyclic reference need to be avoided.
        * For that reason, we are storing parentId instead of pointer to parent NodeData
        */

        [SerializeField] private string id;
        [SerializeField] private string label;
        [SerializeField] private string parentId;
        [SerializeField] private bool isInput;
        [SerializeField] private Rect buttonRect;

        private AssetGraphGuiConfig _assetGraphGuiConfig;

        private AssetGraphGuiConfig assetGraphGuiConfig
        {
            get
            {
                if (_assetGraphGuiConfig == null)
                {
                    _assetGraphGuiConfig = AssetGraphGuiConfig.LoadGraphicsSettings();
                }

                return _assetGraphGuiConfig;
            }
        }

        public ConnectionPointData(string id, string label, NodeData parent, bool isInput /*, int orderPriority, bool showLabel */)
        {
            this.id = id;
            this.label = label;
            this.parentId = parent.Id;
            this.isInput = isInput;
        }

        public ConnectionPointData(string label, NodeData parent, bool isInput)
        {
            this.id = Guid.NewGuid().ToString();
            this.label = label;
            this.parentId = parent.Id;
            this.isInput = isInput;
        }

        public ConnectionPointData(ConnectionPointData p)
        {
            this.id = p.id;
            this.label = p.label;
            this.parentId = p.parentId;
            this.isInput = p.isInput;
            this.buttonRect = p.buttonRect;
        }

        public ConnectionPointData(V1.ConnectionPointData v1)
        {
            this.id = v1.Id;
            this.label = v1.Label;
            this.parentId = v1.NodeId;
            this.isInput = v1.IsInput;
            this.buttonRect = v1.Region;
        }

        public string Id
        {
            get { return id; }
        }

        public string Label
        {
            get { return label; }
            set { label = value; }
        }

        public string NodeId
        {
            get { return parentId; }
        }

        public bool IsInput
        {
            get { return isInput; }
        }

        public bool IsOutput
        {
            get { return !isInput; }
        }

        public Rect Region
        {
            get { return buttonRect; }
        }

        // returns rect for outside marker
        public Rect GetGlobalRegion(NodeGUI node)
        {
            var baseRect = node.Region;
            return new Rect(
                baseRect.x + buttonRect.x,
                baseRect.y + buttonRect.y,
                buttonRect.width,
                buttonRect.height
            );
        }

        // returns rect for connection dot
        public Rect GetGlobalPointRegion(NodeGUI node)
        {
            if (IsInput)
            {
                return GetInputPointRect(node);
            }
            else
            {
                return GetOutputPointRect(node);
            }
        }

        public Vector2 GetGlobalPosition(NodeGUI node)
        {
            var x = 0f;
            var y = 0f;

            var baseRect = node.Region;

            if (IsInput)
            {
                x = baseRect.x + 8f;
                y = baseRect.y + buttonRect.y + (buttonRect.height / 2f) - 1f;
            }

            if (IsOutput)
            {
                x = baseRect.x + baseRect.width;
                y = baseRect.y + buttonRect.y + (buttonRect.height / 2f) - 1f;
            }

            return new Vector2(x, y);
        }

        public void UpdateRegion(NodeGUI node, float yOffset, int index, int max)
        {
            var parentRegion = node.Region;
            if (IsInput)
            {
                var initialY = yOffset + (assetGraphGuiConfig.NODE_BASE_HEIGHT - assetGraphGuiConfig.INPUT_POINT_HEIGHT) / 2f;
                var marginY = initialY + assetGraphGuiConfig.FILTER_OUTPUT_SPAN * (index);

                buttonRect = new Rect(
                    0,
                    marginY,
                    assetGraphGuiConfig.INPUT_POINT_WIDTH,
                    assetGraphGuiConfig.INPUT_POINT_HEIGHT);
            }
            else
            {
                var initialY = yOffset + (assetGraphGuiConfig.NODE_BASE_HEIGHT - assetGraphGuiConfig.OUTPUT_POINT_HEIGHT) / 2f;
                var marginY = initialY +assetGraphGuiConfig.FILTER_OUTPUT_SPAN * (index);

                buttonRect = new Rect(
                    parentRegion.width - assetGraphGuiConfig.OUTPUT_POINT_WIDTH + 1f,
                    marginY,
                    assetGraphGuiConfig.OUTPUT_POINT_WIDTH,
                    assetGraphGuiConfig.OUTPUT_POINT_HEIGHT);
            }
        }

        private Rect GetOutputPointRect(NodeGUI node)
        {
            var baseRect = node.Region;
            return new Rect(
                baseRect.x + baseRect.width - (assetGraphGuiConfig.CONNECTION_POINT_MARK_SIZE) / 2f,
                baseRect.y + buttonRect.y + 1f,
                assetGraphGuiConfig.CONNECTION_POINT_MARK_SIZE,
                assetGraphGuiConfig.CONNECTION_POINT_MARK_SIZE
            );
        }

        private Rect GetInputPointRect(NodeGUI node)
        {
            var baseRect = node.Region;
            return new Rect(
                baseRect.x - 2f,
                baseRect.y + buttonRect.y + 3f,
                assetGraphGuiConfig.CONNECTION_POINT_MARK_SIZE + 3f,
                assetGraphGuiConfig.CONNECTION_POINT_MARK_SIZE + 3f
            );
        }
    }
}