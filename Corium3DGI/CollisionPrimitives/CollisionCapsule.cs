﻿using Corium3D;
using CoriumDirectX;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Corium3DGI.Utils;
using System.Collections.Generic;

namespace Corium3DGI
{
    public class CollisionCapsule : CollisionPrimitive3D
    {
        private const double EPSILON = 1E-6;
        private const string NAME_CACHE = "Capsule";        
        private static Model3DCollection avatars3DCache;
        private static uint[] dxModelIDs;                

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

        private ObservableVector3D axisVec;
        public ObservableVector3D AxisVec
        {
            get { return axisVec; }

            set
            {
                if (axisVec != value)
                {
                    axisVec = value;
                    axisVec.PropertyChanged += OnAxisVecUpdated;
                    OnPropertyChanged("AxisVec");
                }
            }
        }

        private float height;
        public float Height
        {
            get { return height; }

            set
            {
                if (height != value)
                {
                    height = value;
                    OnPropertyChanged("Height");
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
            cacheAvatarsAssets(NAME_CACHE, new Color() { R = 255, G = 255, B = 0, A = 255 }, out avatars3DCache, dxVisualizer, out dxModelIdContainer);
            dxModelIDs = dxModelIdContainer.ToArray();
        }

        public CollisionCapsule(Point3D center, Vector3D axisVec, float height, float radius)
        {
            Name = NAME_CACHE;
            foreach (GeometryModel3D avatar3D in avatars3DCache)
                avatars3D.Add(new GeometryModel3D(avatar3D.Geometry, avatar3D.Material));            

            this.center = new ObservablePoint3D();
            this.axisVec = new ObservableVector3D();            
            this.height = height;
            this.radius = radius;

            ScaleTransform3D scaleTransform3DShaft = new ScaleTransform3D();
            ScaleTransform3D scaleTransform3DDemiSphere1 = new ScaleTransform3D();            
            TranslateTransform3D translateTransform3DDemiSphere1PerHeight = new TranslateTransform3D();            
            ScaleTransform3D scaleTransform3DDemiSphere2 = new ScaleTransform3D();            
            TranslateTransform3D translateTransform3DDemiSphere2PerHeight = new TranslateTransform3D();                        
            TranslateTransform3D translateTransform3D = new TranslateTransform3D();
            bindAvatar3DTransforms(scaleTransform3DShaft, scaleTransform3DDemiSphere1, translateTransform3DDemiSphere1PerHeight,
                                   scaleTransform3DDemiSphere2, translateTransform3DDemiSphere2PerHeight, translateTransform3D);

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            rotateTransform3D.Rotation = new QuaternionRotation3D(Quaternion.Identity);

            Transform3DGroup transform3DGroupShaft = new Transform3DGroup();
            transform3DGroupShaft.Children.Add(scaleTransform3DShaft);
            transform3DGroupShaft.Children.Add(rotateTransform3D);
            transform3DGroupShaft.Children.Add(translateTransform3D);
            Avatars3D[0].Transform = transform3DGroupShaft;

            Transform3DGroup transform3DGroupDemiSphere1 = new Transform3DGroup();
            transform3DGroupDemiSphere1.Children.Add(scaleTransform3DDemiSphere1);
            transform3DGroupDemiSphere1.Children.Add(translateTransform3DDemiSphere1PerHeight);
            transform3DGroupDemiSphere1.Children.Add(rotateTransform3D);
            transform3DGroupDemiSphere1.Children.Add(translateTransform3D);
            Avatars3D[1].Transform = transform3DGroupDemiSphere1;

            Transform3DGroup transform3DGroupDemiSphere2 = new Transform3DGroup();
            transform3DGroupDemiSphere2.Children.Add(scaleTransform3DDemiSphere2);
            transform3DGroupDemiSphere2.Children.Add(translateTransform3DDemiSphere2PerHeight);
            transform3DGroupDemiSphere2.Children.Add(rotateTransform3D);
            transform3DGroupDemiSphere2.Children.Add(translateTransform3D);
            Avatars3D[2].Transform = transform3DGroupDemiSphere2;

            Center.X = center.X; Center.Y = center.Y; Center.Z = center.Z;
            AxisVec.X = axisVec.X; AxisVec.Y = axisVec.Y; AxisVec.Z = axisVec.Z;
        }
        
        public override DxVisualizer.IScene.ISceneModelInstance[] createDxInstances(SceneM sceneM)
        {
            Quaternion axisVecQuat = axisVecToQuat();
            float heightToRadiusRatio = height / radius;
            DxVisualizer.IScene.IConstrainedTransformInstance cylinderDxInstance =
                sceneM.createDxConstrainedTransformInstance(dxModelIDs[0], Color.FromArgb(50, 255, 255, 0),
                                                            (Vector3D)center.Point3DCpy, new Vector3D(radius, height, radius),
                                                            axisVecQuat.Axis, (float)axisVecQuat.Angle, null);
            cylinderDxInstance.setScaleConstraints(DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f,
                                                   DxVisualizer.IScene.TransformScaleConstraint.Factor, height / radius,
                                                   DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f);

            DxVisualizer.IScene.IConstrainedTransformInstance topHemisphereDxInstance =
                sceneM.createDxConstrainedTransformInstance(dxModelIDs[1], Color.FromArgb(50, 255, 255, 0),
                                                            new Vector3D(0.0f, 0.5f, 0.0f),
                                                            new Vector3D(1.0f, 1.0f, 1.0f),
                                                            new Vector3D(1.0f, 0.0f, 0.0f), 0.0f, null);
            topHemisphereDxInstance.setScaleConstraints(DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f,
                                                        DxVisualizer.IScene.TransformScaleConstraint.FollowMaxDimGrp, 0.0f,
                                                        DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f);            
            topHemisphereDxInstance.assignParent(cylinderDxInstance, false);

            DxVisualizer.IScene.IConstrainedTransformInstance bottomHemisphereDxInstance =
                sceneM.createDxConstrainedTransformInstance(dxModelIDs[2], Color.FromArgb(50, 255, 255, 0),
                                                            new Vector3D(0.0f, -0.5f, 0.0f),
                                                            new Vector3D(1.0f, 1.0f, 1.0f),
                                                            new Vector3D(1.0f, 0.0f, 0.0f), 0.0f, null);
            bottomHemisphereDxInstance.setScaleConstraints(DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f,
                                                           DxVisualizer.IScene.TransformScaleConstraint.FollowMaxDimGrp, 0.0f,
                                                           DxVisualizer.IScene.TransformScaleConstraint.MaxDimGrp, 0.0f);            
            bottomHemisphereDxInstance.assignParent(cylinderDxInstance, false);

            return new DxVisualizer.IScene.ISceneModelInstance[] { cylinderDxInstance, topHemisphereDxInstance, bottomHemisphereDxInstance };
        }

        public override void asssignPrimitiveDataInModelAssetGen(AssetsGen.IModelAssetGen modelAssetGen)
        {
            modelAssetGen.assignCollisionCapsule(center.Point3DCpy - 0.5f * axisVec.Vector3DCpy * height, axisVec.Vector3DCpy * height, radius);
        }

        private void bindAvatar3DTransforms(ScaleTransform3D scaleTransform3DShaft, 
                                            ScaleTransform3D scaleTransform3DDemiSphere1, 
                                            TranslateTransform3D translateTransform3DDemiSphere1PerHeight,
                                            ScaleTransform3D scaleTransform3DDemiSphere2,
                                            TranslateTransform3D translateTransform3DDemiSphere2PerHeight,
                                            TranslateTransform3D translateTransform3D)
        {            
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Radius", scaleTransform3DShaft, ScaleTransform3D.ScaleXProperty, null);
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Height", scaleTransform3DShaft, ScaleTransform3D.ScaleYProperty, null);
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Radius", scaleTransform3DShaft, ScaleTransform3D.ScaleZProperty, null);
            
            bindRadiusToScaleTransform(scaleTransform3DDemiSphere1);
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Height", translateTransform3DDemiSphere1PerHeight, TranslateTransform3D.OffsetYProperty, new HalfTheFloat());
            
            bindRadiusToScaleTransform(scaleTransform3DDemiSphere2);           
            bindObjsProperties(this, "Height", translateTransform3DDemiSphere2PerHeight, TranslateTransform3D.OffsetYProperty, new MinusHalfTheFloat());

            axisVec.PropertyChanged += OnAxisVecUpdated;

            TwoWayUpdateOnChangedBinder.bindPoint3DToTranslateTransform(center, translateTransform3D);
        }

        private void bindObjsProperties(object obj, string propertyName, Transform3D transform3D, DependencyProperty transform3DDP, IValueConverter converter)
        {
            Binding objPropertyBinding = new Binding(propertyName);
            objPropertyBinding.Source = obj;
            objPropertyBinding.Mode = BindingMode.TwoWay;
            objPropertyBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            objPropertyBinding.Converter = converter;
            BindingOperations.SetBinding(transform3D, transform3DDP, objPropertyBinding);
        }

        private void bindRadiusToScaleTransform(ScaleTransform3D scaleTransform3D)
        {
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Radius", scaleTransform3D, ScaleTransform3D.ScaleXProperty, null);
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Radius", scaleTransform3D, ScaleTransform3D.ScaleYProperty, null);
            TwoWayUpdateOnChangedBinder.bindObjsProperties(this, "Radius", scaleTransform3D, ScaleTransform3D.ScaleZProperty, null);
        }

        private void OnAxisVecUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (axisVec.Vector3DCpy.LengthSquared < EPSILON)
                return;

            ((QuaternionRotation3D)((RotateTransform3D)((Transform3DGroup)Avatars3D[0].Transform).Children[1]).Rotation).Quaternion = axisVecToQuat();
        }        

        private Quaternion axisVecToQuat()
        {           
            Vector3D axisVecCpy = axisVec.Vector3DCpy;            
            Vector3D upVector = new Vector3D(0, 1, 0);
            Vector3D rotationAxis = Vector3D.CrossProduct(upVector, axisVecCpy);            
            if (rotationAxis.LengthSquared > EPSILON)
            {
                rotationAxis.Normalize();
                double rotationAng = Math.Acos(Vector3D.DotProduct(upVector, axisVecCpy)) * 180.0 / Math.PI;
                return new Quaternion(rotationAxis, rotationAng);
            }
            else
                return Quaternion.Identity;
        }

        private class HalfTheFloat : IValueConverter
        {
            public object Convert(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
            {
                return 0.5 * (float)value;
            }

            public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
            {
                return 2 * (float)value;
            }
        }

        private class MinusHalfTheFloat : IValueConverter
        {
            public object Convert(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
            {
                return -0.5 * (float)value;
            }

            public object ConvertBack(object value, Type targetType, object paramter, System.Globalization.CultureInfo culture)
            {
                return -2 * (float)value;
            }
        }
    }    
}
