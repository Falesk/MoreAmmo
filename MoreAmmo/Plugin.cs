using BepInEx;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using System;
using Smoke;

namespace MoreAmmo
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "falesk.moreammo";
        public const string Name = "More Ammo";
        public const string Version = "1.1.1";
        public void Awake()
        {
            Register.RegisterValues();
            On.RainWorld.OnModsDisabled += delegate (On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
            {
                orig(self, newlyDisabledMods);
                foreach (ModManager.Mod mod in newlyDisabledMods)
                    if (mod.id == GUID)
                    {
                        Register.UnregisterValues();
                        break;
                    }
            };
            On.JokeRifle.AbstractRifle.ctor += delegate (On.JokeRifle.AbstractRifle.orig_ctor orig, JokeRifle.AbstractRifle self, World world, JokeRifle realizedObject, WorldCoordinate pos, EntityID ID, JokeRifle.AbstractRifle.AmmoType ammoType)
            {
                orig(self, world, realizedObject, pos, ID, ammoType);
                self.ammo[new JokeRifle.AbstractRifle.AmmoType("Flies")] = 0;
                self.ammo[new JokeRifle.AbstractRifle.AmmoType("Mold")] = 0;
                self.ammo[new JokeRifle.AbstractRifle.AmmoType("Overseer")] = 0;
                self.ammo[new JokeRifle.AbstractRifle.AmmoType("Shock")] = 0;
                self.ammo[new JokeRifle.AbstractRifle.AmmoType("Haze")] = 0;
            };
            On.JokeRifle.AbstractRifle.AmmoFromString += delegate (On.JokeRifle.AbstractRifle.orig_AmmoFromString orig, JokeRifle.AbstractRifle self, string ammoStr)
            {
                orig(self, ammoStr);
                string[] array = ammoStr.Split(new[] { "<JRa>" }, StringSplitOptions.None);
                self.ammo[Register.Flies] = int.Parse(array[12]);
                self.ammo[Register.Mold] = int.Parse(array[13]);
                self.ammo[Register.Overseer] = int.Parse(array[14]);
                self.ammo[Register.Shock] = int.Parse(array[15]);
                self.ammo[Register.Haze] = int.Parse(array[16]);
            };
            On.JokeRifle.AbstractRifle.AmmoToString += (On.JokeRifle.AbstractRifle.orig_AmmoToString orig, JokeRifle.AbstractRifle self) => orig(self) + $"<JRa>{self.ammo[Register.Flies]}<JRa>{self.ammo[Register.Mold]}<JRa>{self.ammo[Register.Overseer]}<JRa>{self.ammo[Register.Shock]}<JRa>{self.ammo[Register.Haze]}";
            On.JokeRifle.IsValidAmmo += (On.JokeRifle.orig_IsValidAmmo orig, PhysicalObject obj) => orig(obj) || obj is KarmaFlower || obj is FlyLure || obj is OverseerCarcass || obj is JellyFish || obj is Hazer || (obj is SlimeMold mold && !mold.big);
            On.JokeRifle.DoesBulletSpawn += (On.JokeRifle.orig_DoesBulletSpawn orig, JokeRifle self, JokeRifle.AbstractRifle.AmmoType type) => orig(self, type) && type != Register.Flies && type != Register.Overseer;
            On.MoreSlugcats.Bullet.ApplyPalette += delegate (On.MoreSlugcats.Bullet.orig_ApplyPalette orig, Bullet self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                if (self.abstractBullet.bulletType == Register.Shock)
                {
                    self.color = Color.cyan;
                    sLeaser.sprites[0].color = self.color;
                    sLeaser.sprites[1].color = self.color;
                    return;
                }
                if (self.abstractBullet.bulletType == Register.Mold)
                {
                    self.color = Color.yellow;
                    sLeaser.sprites[0].color = self.color;
                    sLeaser.sprites[1].color = self.color;
                    return;
                }
                orig(self, sLeaser, rCam, palette);
            };
            On.MoreSlugcats.Bullet.TerrainImpact += delegate (On.MoreSlugcats.Bullet.orig_TerrainImpact orig, Bullet self, int chunk, IntVector2 direction, float speed, bool firstContact)
            {
                if (firstContact)
                {
                    if (self.abstractBullet.bulletType == Register.Mold)
                    {
                        CreateMold(self);
                        self.room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, self.firstChunk);
                        self.Destroy();
                    }
                    if (self.abstractBullet.bulletType == Register.Haze)
                    {
                        CreateHaze(self);
                        self.Destroy();
                    }
                }
                orig(self, chunk, direction, speed, firstContact);
            };

            On.MoreSlugcats.AmmoMeter.AmmoSprite += AmmoMeter_AmmoSprite;
            On.JokeRifle.AmmoTypeFromObject += JokeRifle_AmmoTypeFromObject;
            On.JokeRifle.ReloadRifle += JokeRifle_ReloadRifle;
            On.MoreSlugcats.Bullet.HitSomething += Bullet_HitSomething;
            On.JokeRifle.Use += JokeRifle_Use;
        }

        private string AmmoMeter_AmmoSprite(On.MoreSlugcats.AmmoMeter.orig_AmmoSprite orig, AmmoMeter self)
        {
            JokeRifle jokeRifle = self.rifleRef();
            if (jokeRifle != null)
            {
                if (jokeRifle.abstractRifle.ammoStyle == Register.Flies)
                {
                    return "Kill_Bat";
                }
                if (jokeRifle.abstractRifle.ammoStyle == Register.Mold)
                {
                    return "Symbol_SlimeMold";
                }
                if (jokeRifle.abstractRifle.ammoStyle == Register.Overseer)
                {
                    return "Kill_Overseer";
                }
                if (jokeRifle.abstractRifle.ammoStyle == Register.Shock)
                {
                    return "Symbol_JellyFish";
                }
                if (jokeRifle.abstractRifle.ammoStyle == Register.Haze)
                {
                    return "Kill_Hazer";
                }
            }
            return orig(self);
        }
        private JokeRifle.AbstractRifle.AmmoType JokeRifle_AmmoTypeFromObject(On.JokeRifle.orig_AmmoTypeFromObject orig, PhysicalObject obj)
        {
            if (obj is KarmaFlower)
            {
                return JokeRifle.AbstractRifle.AmmoType.Void;
            }
            if (obj is FlyLure)
            {
                return Register.Flies;
            }
            if (obj is SlimeMold mold && !mold.big)
            {
                return Register.Mold;
            }
            if (obj is OverseerCarcass)
            {
                return Register.Overseer;
            }
            if (obj is JellyFish)
            {
                return Register.Shock;
            }
            if (obj is Hazer)
            {
                return Register.Haze;
            }
            return orig(obj);
        }
        private void JokeRifle_ReloadRifle(On.JokeRifle.orig_ReloadRifle orig, JokeRifle self, PhysicalObject obj)
        {
            if (!JokeRifle.IsValidAmmo(obj))
                return;
            self.abstractRifle.ammoStyle = JokeRifle.AmmoTypeFromObject(obj);
            var type = self.abstractRifle.ammoStyle;
            self.lastShotTime = 7;
            if (type == Register.Flies)
            {
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 4);
                return;
            }
            if (type == Register.Mold)
            {
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 1);
                return;
            }
            if (type == Register.Overseer)
            {
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 1);
                return;
            }
            if (type == Register.Shock)
            {
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 5);
                return;
            }
            if (type == Register.Haze)
            {
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 3);
                return;
            }
            orig(self, obj);
        }
        private bool Bullet_HitSomething(On.MoreSlugcats.Bullet.orig_HitSomething orig, Bullet self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (!orig(self, result, eu))
                return false;
            if (result.obj is Creature)
            {
                BodyChunk firstChunk = self.firstChunk;
                if (self.abstractBullet.bulletType == Register.Shock)
                {
                    float d = 7f;
                    float damage = 0.1f;
                    (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * d), result.chunk, result.onAppendagePos, Creature.DamageType.Electric, damage, (result.obj is Player) ? 140f : (320f * Mathf.Lerp((result.obj as Creature).Template.baseStunResistance, 1f, 0.5f)));
                    self.room.AddObject(new CreatureSpasmer(result.obj as Creature, false, (result.obj as Creature).stun));
                    if (ModManager.MSC && result.obj is Player && (result.obj as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    {
                        (result.obj as Player).SaintStagger(520);
                    }
                    self.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, firstChunk.pos);
                    self.room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
                    self.Destroy();
                }
                else if (self.abstractBullet.bulletType == Register.Mold)
                {
                    float d = 7f;
                    float damage = 0.2f;
                    float stunBonus = 45f;
                    CreateMold(self);
                    (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * d), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, damage, stunBonus);
                    self.Destroy();
                }
                else if (self.abstractBullet.bulletType == Register.Haze)
                {
                    float d = 7f;
                    float damage = 0.01f;
                    float stunBonus = 45f;
                    (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * d), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, damage, stunBonus);
                    CreateHaze(self);
                    self.Destroy();
                }
            }
            return true;
        }

        private void JokeRifle_Use(On.JokeRifle.orig_Use orig, JokeRifle self, bool eu)
        {
            var type = self.abstractRifle.ammoStyle;
            bool flag = type == Register.Flies || type == Register.Mold || type == Register.Overseer || type == Register.Shock || type == Register.Haze;
            if (flag && self.abstractRifle.currentAmmo() > 0)
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 1);
            orig(self, eu);
            if (flag)
            {
                if (self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && type == Register.Flies)
                {
                    self.counter = 40;
                    AbstractCreature abstr = new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, self.room.GetWorldCoordinate(self.firePos), self.room.game.GetNewID());
                    self.room.abstractRoom.AddEntity(abstr);
                    abstr.RealizeInRoom();
                    Fly fly = abstr.realizedCreature as Fly;
                    fly.firstChunk.HardSetPosition(self.firePos);
                    fly.Die();
                    fly.mainBodyChunk.vel = self.aimDir * 20f;
                }
                else if (self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && type == Register.Overseer)
                {
                    self.counter = 40;
                    AbstractCreature abstr = new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, self.room.GetWorldCoordinate(self.firePos), self.room.game.GetNewID());
                    self.room.abstractRoom.AddEntity(abstr);
                    abstr.RealizeInRoom();
                    Overseer overseer = abstr.realizedCreature as Overseer;
                    (abstr.abstractAI as OverseerAbstractAI).ownerIterator = UnityEngine.Random.Range(0, 6);
                    overseer.firstChunk.HardSetPosition(self.firePos);
                    overseer.mainBodyChunk.vel = self.aimDir * 5f;
                }
                else if (self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && type == Register.Mold)
                    self.counter = 50;
                else if (self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && type == Register.Shock)
                    self.counter = 15;
                else if (self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && type == Register.Haze)
                    self.counter = 60;
                self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() - 1);
                if (self.abstractRifle.currentAmmo() < 0)
                    self.abstractRifle.setCurrentAmmo(self.abstractRifle.currentAmmo() + 1);
            }
        }

        public void CreateMold(Bullet bullet)
        {
            AbstractConsumable abstr = new AbstractConsumable(bullet.room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, bullet.room.GetWorldCoordinate(bullet.firstChunk.pos), bullet.room.game.GetNewID(), 0, 0, null);
            bullet.room.abstractRoom.AddEntity(abstr);
            abstr.RealizeInRoom();
            (abstr.realizedObject as SlimeMold).firstChunk.pos = bullet.firstChunk.pos;
            (abstr.realizedObject as SlimeMold).firstChunk.lastPos = bullet.firstChunk.lastPos;
            (abstr.realizedObject as SlimeMold).big = true;
        }
        public void CreateHaze(Bullet bullet)
        {
            smoke = new BlackHaze(bullet.room, bullet.firstChunk.pos);
            bullet.room.AddObject(smoke);
            smoke.EmitSmoke(bullet.firstChunk.pos, 1f);
            smoke.EmitBigSmoke(Mathf.InverseLerp(2f, 0f, 0f));
            bullet.room.PlaySound(SoundID.Puffball_Eplode, bullet.firstChunk.pos);
        }

        public BlackHaze smoke;
    }
}
