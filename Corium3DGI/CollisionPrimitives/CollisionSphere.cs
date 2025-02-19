﻿using Corium3D;
using CoriumDirectX;

using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Corium3DGI.Utils;
using System.Collections.Generic;

namespace Corium3DGI
{
    public class CollisionSphere : CollisionPrimitive3D
    {
        private const string NAME_CACHE = "Sphere";        
        private static Model3DCollection avatars3DCache;
        private static uint dxModelID;
        

        private ObservablePoint3D center;
        public ObservablePoint3D Center
        {
            get { return center; }

            set
            {
                if (center != value)
                {
                    center = value;
                    OnPropertyChanged("Center");
                }
            }
        }

        private float radius;
        public float Radius
        {
            get { return radius; }

            set
            {
                if (radius != value)
                {
                    radius = value;
                    OnPropertyChanged("Radius");
                }
            }
        }
        
        public static void Init(DxVisualizer dxVisualizer)
        {
            List<uint> dxModelIdContainer;
            cacheAvatarsAssets(NAME_CACHE, new Color() { R = 0, G = 0, B = 255, A = 255 }, out avatars3DCache, dxVisualizer, out dxModelIdContainer);
            dxModelID = dxModelIdContainer[0];
        }

        public CollisionSphere(Point3D center, float radius) {
            Name = NAME_CACHE;
            foreach (GeometryModel3D avatar3D in avatars3DCache)
                avatars3D.Add(new GeometryModel3D(avatar3D.Geometry, avatar3D.Material));            

            this.center = new ObservablePoint3D();
            this.radius = radius;

            ScaleTransform3D scaleTransform3D = new ScaleTransform3D();
            bindRadiusToScaleTransform(scaleTransform3D);
            TranslateTransform3D translateTransform3D = new TranslateTransform3D();
            bindCenterToTranslateTransform(translateTransform3D);
            Transform3DGroup transform3DGroup = new Transform3DGroup();
            transform3DGroup.Children.Add(scaleTransform3D);
            transform3DGroup.Children.Add(translateTransform3D);
            Avatars3D[0].Transform = transform3DGroup;

            Center.X = center.X; Center.Y = center.Y; Center.Z = center.Z;
        }

        public override DxVisualizer.IScene.ISceneModelInstance[] createDxInstances(SceneM sceneM)
        {
            DxVisualizer.IScene.IConstrainedTransformInstance sphereDxInstance = 
                sceneM.createDxConstrainedTransformInstance(dxModelID, Color.FromArgb(50, 0, 0, 255), 
                                                            (Vector3D)center.Point3DCpy, 
                                                            new Vector3D(radius, radius, radius), 
                                                            new Vector3D(0.0f, 0.0f, 1.0f), 0.0f, null);
            sphereDxInstance.setScaleConstraints(DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f,
                                                 DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f,
                                                 DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f);

            return new DxVisualizer.IScene.ISceneModelInstance[] { sphereDxInstance };            
        }

        public override void asssignPrimitiveDataInModelAssetGen(AssetsGen.IModelAssetGen modelAssetGen)
        {
            modelAssetGen.assignCollisionSphere(center.Point3DCpy, radius);
        }

        private void bindRadiusToScaleTransform(ScaleTransform3D scaleTransform3D)
        {
            bindRadiusToScaleTransformCoord(scaleTransform3D, ScaleTransform3D.ScaleXProperty);
            bindRadiusToScaleTransformCoord(scaleTransform3D, ScaleTransform3D.ScaleYProperty);
            bindRadiusToScaleTransformCoord(scaleTransform3D, ScaleTransform3D.ScaleZProperty);
        }

        private void bindRadiusToScaleTransformCoord(ScaleTransform3D scaleTransform3D, DependencyProperty coordDP)
        {
            Binding radiusBinding = new Binding("Radius");
            radiusBinding.Source = this;
            radiusBinding.Mode = BindingMode.TwoWay;
            radiusBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(scaleTransform3D, coordDP, radiusBinding);
        }

        private void bindCenterToTranslateTransform(TranslateTransform3D translateTransform3D)
        {
            bindCenterCoordToTranslateTransformCoord(translateTransform3D, "X", TranslateTransform3D.OffsetXProperty);
            bindCenterCoordToTranslateTransformCoord(translateTransform3D, "Y", TranslateTransform3D.OffsetYProperty);
            bindCenterCoordToTranslateTransformCoord(translateTransform3D, "Z", TranslateTransform3D.OffsetZProperty);
        }

        private void bindCenterCoordToTranslateTransformCoord(TranslateTransform3D translateTransform3D, string coordName, DependencyProperty coordDP)
        {
            Binding coordBinding = new Binding(coordName);
            coordBinding.Source = center;
            coordBinding.Mode = BindingMode.TwoWay;
            coordBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(translateTransform3D, coordDP, coordBinding);
        }
    }
}
