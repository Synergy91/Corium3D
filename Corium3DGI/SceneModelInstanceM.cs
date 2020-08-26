﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using Corium3DGI.Utils;
using CoriumDirectX;

namespace Corium3DGI
{
    public class SceneModelInstanceM : ObservableObject
    {
        private int instanceIdx;
        private DxVisualizer.IScene.SelectionHandler selectionHandler;
        private OnSceneModelInstanceSelected selectionHandlerExt;        

        public DxVisualizer.IScene.ISceneModelInstance IDxSceneModelInstance { get; private set; }
        
        public SceneModelM SceneModelMRef { get; }

        public ModelVisual3D ModelVisual3DRef { get; }

        private string name;
        public string Name
        {
            get { return name; }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public ObservableVector3D Translate { get; }

        public ObservableVector3D Scale { get; }

        public ObservableQuaternion Rot { get; }

        private bool isShown = true;
        public bool IsShown
        {
            get { return isShown; }

            set
            {
                if (isShown != value)
                {
                    isShown = value;
                    OnPropertyChanged("IsShown");
                }
            }
        }

        public delegate void OnSceneModelInstanceSelected(SceneModelInstanceM thisSelected);        

        public SceneModelInstanceM(SceneModelM sceneModel, int instanceIdx, Vector3D translate, Vector3D scale, Vector3D rotAx, float rotAng, OnSceneModelInstanceSelected selectionHandlerExt)
        {
            selectionHandler = new DxVisualizer.IScene.SelectionHandler(onSelected);
            this.selectionHandlerExt = selectionHandlerExt;
            this.instanceIdx = instanceIdx;
            SceneModelMRef = sceneModel;                        
            name = sceneModel.Name + instanceIdx.ToString();
            Translate = new ObservableVector3D(translate);
            Scale = new ObservableVector3D(scale);
            Rot = new ObservableQuaternion(rotAx, rotAng);
            
            IDxSceneModelInstance = sceneModel.IDxScene.createModelInstance(sceneModel.ModelMRef.DxModelID, translate, scale, rotAx, rotAng, selectionHandler);            
            Translate.PropertyChanged += onTranslationChanged;
            Scale.PropertyChanged += onScaleChanged;
            Rot.PropertyChanged += onRotationChanged;
        }

        public int getInstanceIdx() { return instanceIdx; }

        public void releaseDxLmnts()
        {
            IDxSceneModelInstance.release();            
        }

        public void highlight()
        {
            IDxSceneModelInstance.highlight();
        }

        public void dim()
        {
            IDxSceneModelInstance.dim();
        }

        public void toggleVisibility()
        {
            IsShown = !IsShown;

            if (IsShown)
                IDxSceneModelInstance.show();
            else
                IDxSceneModelInstance.hide();            
        }

        private void onSelected()
        {            
            selectionHandlerExt(this);
        }

        private void onTranslationChanged(object sender, PropertyChangedEventArgs e)
        {
            IDxSceneModelInstance.setTranslation(Translate.Vector3DCpy);
        }

        private void onScaleChanged(object sender, PropertyChangedEventArgs e)
        {
            IDxSceneModelInstance.setScale(Scale.Vector3DCpy);
        }

        private void onRotationChanged(object sender, PropertyChangedEventArgs e)
        {
            IDxSceneModelInstance.setRotation(Rot.Axis.Vector3DCpy, (float)Rot.Angle);
        }        
    }
}
