﻿using GameDevelopment.GameState;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDevelopment.GameObject
{
    public class MovementManager
    {
        private int BoundsX;
        private int BoundsY;
        private int EntitySizeX;
        private int EntitySizeY;

        private float JumpHeight = 5;
        private float Friction = 0.99f;
        private float Gravity = 0.10f;
        private float BaseSpeed = 4f;
        public MovementManager(int boundsX, int boundsY, int sizeX, int sizeY, float jumpHeight = 10, float friction = 1f, float gravity = 0.80f, float baseSpeed = 4f)
        {
            BoundsX = boundsX;
            BoundsY = boundsY;
            EntitySizeX = sizeX;
            EntitySizeY = sizeY;
            JumpHeight = jumpHeight;
            Friction = friction;
            Gravity = gravity;
            BaseSpeed = baseSpeed;
        }
        private bool IsGrounded(IMovable movable)
        {
            if (movable.Position.Y == (BoundsY - EntitySizeY))
                return true;

            foreach (var block in StateManager.getInstance().GetBlocks())
            {
                Rectangle boundingBox = block.BoundingBox;
                boundingBox.Inflate(0, 1);
                if (movable.BoundingBox.Intersects(boundingBox))
                {
                    if (movable.BoundingBox.Bottom == block.BoundingBox.Top)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public void Update(IMovable movable, Vector2 direction)
        {
            if (direction.Y == 1f && !IsGrounded(movable)) Accelerate(movable, 0, BaseSpeed); // Ctrl
            else if (direction.Y == -1f && movable.Speed.Y == 0f && IsGrounded(movable)) Accelerate(movable, 0, -JumpHeight); // Space
            if (direction.X == -1f) Move(movable, -BaseSpeed, 0); // Left
            else if (direction.X == 1f) Move(movable, BaseSpeed, 0); // Right

            Move(movable, movable.Speed.X, movable.Speed.Y);
            movable.Speed = new Vector2(movable.Speed.X * Friction, movable.Speed.Y * Friction);
            Accelerate(movable, 0, Gravity);

            ClampBounds(movable);

            if (IsGrounded(movable)) movable.Speed = new Vector2(movable.Speed.X, Math.Min(0, movable.Speed.Y)); // Kill gravity if grounded

            UpdateState(movable, direction);
        }
        private void Accelerate(IMovable movable, float accelX, float accelY)
        {
            movable.Speed = new Vector2(movable.Speed.X + accelX, movable.Speed.Y + accelY);
        }
        private void Move(IMovable movable, float deltaX, float deltaY)
        {
            movable.Position = new Vector2(movable.Position.X + deltaX, movable.Position.Y + deltaY);

            bool touchingBounds = movable.Position.Y == 0f;
            if (touchingBounds)
            {
                movable.Speed = new Vector2(0, 0);
            }
        }
        private void ClampBounds(IMovable movable)
        {
            if (movable.Position.X < 0 || movable.Position.Y < 0 || movable.Position.X > (BoundsX - EntitySizeX) || movable.Position.Y > (BoundsY - EntitySizeY))
            {
                if (movable.Position.X < 0) movable.Speed = new Vector2(Math.Min(0, movable.Speed.X), movable.Speed.Y);
                else if (movable.Position.X > (BoundsX - EntitySizeX)) movable.Speed = new Vector2(Math.Min(0, movable.Speed.X), movable.Speed.Y);
                if (movable.Position.Y < 0) movable.Speed = new Vector2(movable.Speed.X, Math.Min(0, movable.Speed.Y));
                else if (movable.Position.Y > (BoundsY - EntitySizeY)) movable.Speed = new Vector2(movable.Speed.X, Math.Min(0, movable.Speed.Y));
                movable.Position = new Vector2(Math.Clamp(movable.Position.X, 0, BoundsX - EntitySizeX), Math.Clamp(movable.Position.Y, 0, BoundsY - EntitySizeY));
            }

            foreach (var block in StateManager.getInstance().GetBlocks())
            {
                if (movable.BoundingBox.Intersects(block.BoundingBox))
                {
                    Rectangle collision = Rectangle.Intersect(movable.BoundingBox, block.BoundingBox);

                    if (collision.Width > collision.Height) // Only clamp on shallow axis to avoid teleporting
                    {
                        if (movable.BoundingBox.Bottom > block.BoundingBox.Top && movable.BoundingBox.Bottom < block.BoundingBox.Bottom) // Object is above
                        {
                            movable.Speed = new Vector2(movable.Speed.X, Math.Min(0, movable.Speed.Y));
                            movable.Position += new Vector2(0, block.BoundingBox.Top - movable.BoundingBox.Bottom);
                        }
                        else if (movable.BoundingBox.Top < block.BoundingBox.Bottom && movable.BoundingBox.Top > block.BoundingBox.Top) // Object is below
                        {
                            movable.Speed = new Vector2(movable.Speed.X, Math.Max(0, movable.Speed.Y));
                            movable.Position += new Vector2(0, block.BoundingBox.Bottom - movable.BoundingBox.Top);
                        }
                    }
                    else
                    {
                        // Kill vertical momentum on touching the side of a platform using Math.Max(0, movable.Speed.Y)
                        // This is the same behavior as Super Mario Bros
                        float newSpeedY = movable.Speed.Y;
                        if (Configuration.stopVerticalMomentum)
                            newSpeedY = Math.Max(0, newSpeedY);
                        if (movable.BoundingBox.Left < block.BoundingBox.Right && movable.BoundingBox.Left > block.BoundingBox.Left) // Object is right
                        {
                            movable.Speed = new Vector2(Math.Min(0, movable.Speed.X), newSpeedY);
                            movable.Position += new Vector2(block.BoundingBox.Right - movable.BoundingBox.Left, 0);
                        }
                        else if (movable.BoundingBox.Right > block.BoundingBox.Left && movable.BoundingBox.Right < block.BoundingBox.Right) // Object is left
                        {
                            movable.Speed = new Vector2(Math.Min(0, movable.Speed.X), newSpeedY);
                            movable.Position += new Vector2(block.BoundingBox.Left - movable.BoundingBox.Right, 0);
                        }
                    }
                }
            }
        }
        private void UpdateState(IMovable movable, Vector2 direction)
        {
            if (movable.Speed == Vector2.Zero)
                if (direction.Y == 1)
                    movable.State = IMovable.MovableState.Falling;
                else
                    movable.State = IMovable.MovableState.Idle;
            else if (movable.Speed.Y == 0)
                movable.State = IMovable.MovableState.Walking;
            else if (movable.Speed.Y < 0)
                movable.State = IMovable.MovableState.Jumping;
            else if (movable.Speed.Y > 0)
                movable.State = IMovable.MovableState.Falling;
        }
    }
}
