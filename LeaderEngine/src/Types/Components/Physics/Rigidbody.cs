﻿using BepuPhysics;
using BepuPhysics.Collidables;

namespace LeaderEngine
{
    public class Rigidbody : Component
    {
        private Collider collider;

        private BodyHandle handle;

        public override void Start()
        {
            collider = BaseEntity.GetComponent<Collider>();
            collider.Shape.ComputeInertia(1.0f, out var bodyInertia);

            handle = PhysicsController.Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new System.Numerics.Vector3(Transform.LocalPosition.X, Transform.LocalPosition.Y, Transform.LocalPosition.Z),
                bodyInertia,
                new CollidableDescription(collider.ShapeIndex, 0.01f),
                new BodyActivityDescription(0.05f)));

            PhysicsController.OnPhysicsUpdate += OnPhysicsUpdate;
        }

        private void OnPhysicsUpdate(Simulation sim)
        {
            BodyReference body = sim.Bodies.GetBodyReference(handle);

            Transform.LocalPosition = new OpenTK.Mathematics.Vector3(body.Pose.Position.X, body.Pose.Position.Y, body.Pose.Position.Z);
            Transform.Rotation = new OpenTK.Mathematics.Quaternion(body.Pose.Orientation.X, body.Pose.Orientation.Y, body.Pose.Orientation.Z, body.Pose.Orientation.W);
        }

        public override void OnRemove()
        {
            PhysicsController.OnPhysicsUpdate -= OnPhysicsUpdate;

            PhysicsController.Simulation.Bodies.Remove(handle);
        }
    }
}
