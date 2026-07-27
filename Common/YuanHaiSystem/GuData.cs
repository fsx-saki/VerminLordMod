using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.YuanHaiSystem
{
    public enum GuPersonality
    {
        Drifter,    // 漂流：缓慢随机漂移
        Flutter,    // 扑动：快速小范围抖动
        Circler,    // 盘旋：绕圈游动
        Swayer,     // 摇摆：8字摆动
        Stalker     // 潜行：缓慢直线+突然加速
    }

    public class GuData
    {
        public int ItemType;
        public bool Refined;
        public GuPersonality Personality;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float AnimTimer;
        public float TargetRotation;

        public GuData() { }

        public GuData(int itemType, GuPersonality personality)
        {
            ItemType = itemType;
            Refined = true;
            Personality = personality;
            Position = new Vector2(
                Main.rand?.Next(-200, 201) ?? 0,
                Main.rand?.Next(-200, 201) ?? 0);
            Velocity = Vector2.Zero;
            Rotation = 0;
            AnimTimer = Main.rand?.Next(1000) ?? 0;
        }

        public void UpdateMovement()
        {
            AnimTimer += 0.016f * 60;
            float dt = 0.016f * 60;

            switch (Personality)
            {
                case GuPersonality.Drifter:
                    Velocity += new Vector2(
                        (float)Math.Sin(AnimTimer * 0.02f + ItemType) * 0.02f,
                        (float)Math.Cos(AnimTimer * 0.025f + ItemType * 2) * 0.02f);
                    Velocity *= 0.98f;
                    TargetRotation = Velocity.X * 0.5f;
                    break;

                case GuPersonality.Flutter:
                    Velocity += new Vector2(
                        (float)Math.Sin(AnimTimer * 0.1f) * 0.1f,
                        (float)Math.Cos(AnimTimer * 0.12f + 1) * 0.1f);
                    Velocity *= 0.92f;
                    TargetRotation = (float)Math.Sin(AnimTimer * 0.08f) * 0.3f;
                    break;

                case GuPersonality.Circler:
                    float radius = 40 + (float)Math.Sin(ItemType * 0.5f) * 20;
                    float speed = 0.015f + (ItemType % 10) * 0.002f;
                    Vector2 center = new(0, 0);
                    Vector2 desired = center + new Vector2(
                        (float)Math.Cos(AnimTimer * speed) * radius,
                        (float)Math.Sin(AnimTimer * speed) * radius);
                    Velocity += (desired - Position) * 0.01f;
                    Velocity *= 0.97f;
                    TargetRotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
                    break;

                case GuPersonality.Swayer:
                    float sx = (float)Math.Sin(AnimTimer * 0.03f) * 60;
                    float sy = (float)Math.Sin(AnimTimer * 0.06f) * 30;
                    Vector2 swayTarget = new(sx, sy);
                    Velocity += (swayTarget - Position) * 0.005f;
                    Velocity *= 0.96f;
                    TargetRotation = (float)Math.Sin(AnimTimer * 0.04f) * 0.5f;
                    break;

                case GuPersonality.Stalker:
                    if (AnimTimer % 180 < 10)
                    {
                        Velocity += new Vector2(
                            (float)Math.Sin(ItemType * 3.7f) * 0.3f,
                            (float)Math.Cos(ItemType * 5.1f) * 0.3f);
                    }
                    Velocity *= 0.99f;
                    TargetRotation = Velocity.Length() > 0.5f
                        ? (float)Math.Atan2(Velocity.Y, Velocity.X)
                        : (float)Math.Sin(AnimTimer * 0.02f) * 0.2f;
                    break;
            }

            Position += Velocity * dt;
            Rotation += (TargetRotation - Rotation) * 0.05f;
        }

        public TagCompound Serialize()
        {
            return new TagCompound
            {
                ["ItemType"] = ItemType,
                ["Refined"] = Refined,
                ["Personality"] = (int)Personality,
                ["PosX"] = Position.X,
                ["PosY"] = Position.Y
            };
        }

        public static GuData Deserialize(TagCompound tag)
        {
            return new GuData
            {
                ItemType = tag.GetInt("ItemType"),
                Refined = tag.GetBool("Refined"),
                Personality = (GuPersonality)tag.GetInt("Personality"),
                Position = new Vector2(tag.GetFloat("PosX"), tag.GetFloat("PosY")),
                Velocity = Vector2.Zero,
                Rotation = 0,
                AnimTimer = Main.rand?.Next(1000) ?? 0
            };
        }
    }
}
