﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using Hazel;
using HarmonyLib;
using System.Linq;
using System.Text;

namespace Nebula
{
    public static class Helpers
    {
        public static bool ShowButtons
        {
            get
            {
                return !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                      !MeetingHud.Instance &&
                      !ExileController.Instance;
            }
        }
        public static bool ProceedTimer(bool isImpostorKillButton)
        {

            if (PlayerControl.LocalPlayer.inVent) return false;
            if (MeetingHud.Instance) return false;

            //情報端末以外ではカウントが進む
            if (MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                return !MapBehaviour.Instance.countOverlay.isActiveAndEnabled;

            if (Minigame.Instance)
            {
                if (isImpostorKillButton) return false;
                if (Minigame.Instance.TryCast<SpawnInMinigame>()) return false;
                if (Minigame.Instance.MyNormTask) return true;
                if (Minigame.Instance.TryCast<DoorCardSwipeGame>()) return true;
                if (Minigame.Instance.TryCast<DoorBreakerGame>()) return true;
            }

            return PlayerControl.LocalPlayer.CanMove;
        }

        public static Sprite loadSpriteFromResources(Texture2D texture, float pixelsPerUnit, Rect textureRect)
        {
            return Sprite.Create(texture, textureRect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static Sprite loadSpriteFromResources(Texture2D texture, float pixelsPerUnit, Rect textureRect, Vector2 pivot)
        {
            return Sprite.Create(texture, textureRect, pivot, pixelsPerUnit);
        }

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static string loadTextFromResources(string path)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteArray = new byte[stream.Length];
                var read = stream.Read(byteArray, 0, (int)stream.Length);
                return Encoding.Unicode.GetString(byteArray);
            }
            catch
            {
                
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static bool isCustomServer()
        {
            if (DestroyableSingleton<ServerManager>.Instance == null) return false;
            StringNames n = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }

        public static void clearAllTasks(this PlayerControl player)
        {
            if (player == null) return;
            for (int i = 0; i < player.myTasks.Count; i++)
            {
                PlayerTask playerTask = player.myTasks.get_Item(i);
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();

            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();

        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value)
        {
            float alpha = value ? 0.25f : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.NameText.color = new Color(player.NameText.color.r, player.NameText.color.g, player.NameText.color.b, alpha);
        }

        public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts)
        {
            return t.GetString(key, parts);
        }

        public static string csTop(Color c)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a));
        }

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
        {
            tie = true;
            KeyValuePair<byte, int> result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, int> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }

        public static void SetLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId)
        {
            target.RawSetVisor(visorId);
            target.RawSetHat(hatId, colorId);
            target.RawSetSkin(skinId,colorId);
            target.RawSetColor(colorId);
            PlayerControl.SetPlayerMaterialColors(colorId, target.VisorSlot.Image);

            Game.GameData.data.players[target.PlayerId].currentName = playerName;

            /*
            SkinData nextSkin = DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId);
            PlayerPhysics playerPhysics = target.MyPhysics;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.RunAnim) clip = nextSkin.viewData.viewData.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.SpawnAnim) clip = nextSkin.viewData.viewData.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.EnterVentAnim) clip = nextSkin.viewData.viewData.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.ExitVentAnim) clip = nextSkin.viewData.viewData.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.IdleAnim) clip = nextSkin.viewData.viewData.IdleAnim;
            else clip = nextSkin.viewData.viewData.IdleAnim;
            float progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            playerPhysics.Skin.skin = nextSkin.viewData.viewData;
            spriteAnim.Play(clip, 1f);
            spriteAnim.m_animator.Play("a", 0, progress % 1);
            spriteAnim.m_animator.Update(0f);
            */

            //死体のペットは変更しない(生き返ってしまうため)
            if (target.Data.IsDead) return;
            if (target.CurrentPet) UnityEngine.Object.Destroy(target.CurrentPet.gameObject);
            target.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.Instance.GetPetById(petId).viewData.viewData);
            target.CurrentPet.transform.position = target.transform.position;
            target.CurrentPet.Source = target;
            target.CurrentPet.Visible = target.Visible;
            PlayerControl.SetPlayerMaterialColors(colorId, target.CurrentPet.rend);
        }

        public static void SetOutfit(this PlayerControl target, string name,Game.PlayerData.PlayerOutfitData outfit)
        {
            if (outfit == null)
            {
                return;
            }

            target.SetLook(name, outfit.ColorId, outfit.HatId, outfit.VisorId, outfit.SkinId, outfit.PetId);
        }

        public static void SetOutfit(this PlayerControl target, PlayerControl reference)
        {
            string name = Game.GameData.data.players[reference.PlayerId].name;
            Game.PlayerData.PlayerOutfitData outfit = Game.GameData.data.players[reference.PlayerId].CurrentOutfit;
            if (outfit == null)
            {
                return;
            }

            target.SetLook(name, outfit.ColorId, outfit.HatId, outfit.VisorId, outfit.SkinId, outfit.PetId);
        }

        public static void ResetOutfit(this PlayerControl target)
        {
            target.SetOutfit(target.GetModData().name, target.GetModData().Outfit);
        }

        public static Game.PlayerData? GetModData(byte player)
        {
            if (Game.GameData.data.players.ContainsKey(player))
            {
                return Game.GameData.data.players[player];
            }
            return null;
        }

        public static bool HasModData(byte player)
        {
            if (Game.GameData.data == null) return false; 
            return Game.GameData.data.players.ContainsKey(player);
        }

        public static Game.PlayerData? GetModData(this PlayerControl player)
        {
            return GetModData(player.PlayerId);
        }

        public static Game.PlayerData? GetModData(this GameData.PlayerInfo player)
        {
            return GetModData(player.PlayerId);
        }

        public static Game.PlayerData? GetModData(this DeadBody player)
        {
            return GetModData(player.ParentId);
        }

        public static DeadBody[] AllDeadBodies()
        {
            return UnityEngine.Object.FindObjectsOfType<DeadBody>();
        }

        public static float Distance(this Vector3 vector,Vector3 opponent)
        {
            float x = vector.x - opponent.x;
            float y = vector.y - opponent.y;
            return Mathf.Sqrt(x*x + y*y);
        }

        public static void shareGameVersion()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
            writer.Write(NebulaPlugin.Instance.PluginVersionData.Length);
            foreach (byte data in NebulaPlugin.Instance.PluginVersionData)
            {
                writer.Write(data);
            }
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCEvents.VersionHandshake(NebulaPlugin.Instance.PluginVersionData, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
        }

        public static Vector3 GetVector(float radius)
        {
            return GetVector((float)(NebulaPlugin.rnd.NextDouble() * Math.PI * 2f), radius);
        }

        public static Vector3 GetVector(float angle, float radius)
        {
            return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        public enum MurderAttemptResult
        {
            PerformKill,
            SuppressKill,
            BlankKill
        }

        public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false)
        {
            MurderAttemptResult result= MurderAttemptResult.PerformKill;
            //GlobalMethod
            result=target.GetModData().role.OnMurdered(killer.PlayerId, target.PlayerId);
            if (result != MurderAttemptResult.PerformKill)
            {
                return result;
            }

            return MurderAttemptResult.PerformKill;
        }
        public static MurderAttemptResult checkMuderAttemptAndKill(PlayerControl killer, PlayerControl target,Game.PlayerData.PlayerStatus status, bool isMeetingStart = false, bool showAnimation = true)
        {
            MurderAttemptResult murder = checkMuderAttempt(killer, target, isMeetingStart);
            if (murder == MurderAttemptResult.PerformKill)
            {
                RPCEventInvoker.UncheckedMurderPlayer(killer.PlayerId,target.PlayerId, status.Id, showAnimation);
            }
            return murder;
        }

        public static void PlayCustomFlash(Color color,float fadeIn ,float fadeOut,float maxAlpha=0.5f)
        {
            float duration = fadeIn + fadeOut;

            var flash = GameObject.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.transform);
            flash.color = color;
            flash.enabled = true;
            flash.gameObject.active = true;

            HudManager.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                if (p < (fadeIn/duration))
                {
                    if (flash != null)
                        flash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(maxAlpha * p / (fadeIn / duration)));
                }
                else
                {
                    if (flash != null)
                        flash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(maxAlpha * (1 - p) / (fadeOut / duration)));
                }
                if (p == 1f && flash != null) { 
                    flash.enabled = false;
                    GameObject.Destroy(flash.gameObject);
                }
            })));
        }

        public static void PlayFlash(Color color)
        {
            PlayCustomFlash(color,0.375f, 0.375f);
        }

        public static void PlayQuickFlash(Color color)
        {
            PlayCustomFlash(color, 0.1f, 0.4f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chance">0～10の間で指定</param>
        /// <returns></returns>
        static public int CalcProbabilityCount(int chance,int max)
        {
            if (max == 0) { return 0; }

            int count = 0;
            double rate = (double)chance / 10.0;
            for(int i = 0; i < max; i++)
            {
                if (NebulaPlugin.rnd.NextDouble() < rate) count++;
            }
            return count;
        }

        static public int[] GetRandomArray(int length)
        {
            int[] arr = new int[length];
            for(int i = 0; i < length; i++)
            {
                arr[i] = i;
            }

            System.Random random = new System.Random();
            arr = arr.OrderBy(x => random.Next()).ToArray();

            return arr;
        }

        static public Type[] GetRandomArray<Type>(ICollection<Type> collection)
        {
            Type[] arr = new Type[collection.Count];
            int index = 0;
            foreach (Type value in collection)
            {
                arr[index] = value;
                index++;
            }

            System.Random random = new System.Random();
            arr = arr.OrderBy(x => random.Next()).ToArray();

            return arr;
        }

        static public void RoleAction(Game.PlayerData player, System.Action<Roles.Assignable> action)
        {
            action.Invoke(player.role);
            foreach (Roles.ExtraRole role in player.extraRole)
            {
                action.Invoke(role);
            }

        }
        static public void RoleAction(byte playerId, System.Action<Roles.Assignable> action)
        {
            Game.PlayerData data = Game.GameData.data.players[playerId];
            action.Invoke(data.role);
            foreach (Roles.ExtraRole role in data.extraRole)
            {
                action.Invoke(role);
            }
        }

        static public void RoleAction(PlayerControl player, System.Action<Roles.Assignable> action)
        {
            RoleAction(player.PlayerId, action);
        }

        static public Game.VentData GetVentData(this Vent vent)
        {
            if (vent == null) return null;
            if (Game.GameData.data == null) return null;
            return Game.GameData.data.GetVentData(vent.gameObject.name);
        }
        
        static public bool SabotageIsActive()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                    return true;
            return false;
        }

        static public void RepairSabotage()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.TaskType == TaskTypes.FixLights)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FixLights, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCEvents.FixLights();
                }
                else if (task.TaskType == TaskTypes.RestoreOxy)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                }
                else if (task.TaskType == TaskTypes.ResetReactor)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                }
                else if (task.TaskType == TaskTypes.ResetSeismic)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                }
                else if (task.TaskType == TaskTypes.FixComms)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                }
                else if (task.TaskType == TaskTypes.StopCharles)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                }
            }
        }

        static public Texture2D CreateReadabeTexture(Texture texture)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                        texture.width,
                        texture.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D readableTextur2D = new Texture2D(texture.width, texture.height);
            readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTextur2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            
            return readableTextur2D;
        }

        public static void destroyList<T>(Il2CppSystem.Collections.Generic.List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }

        public static void destroyList<T>(List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }

        public static void ShowDialog(string text)
        {
            HudManager.Instance.Dialogue.transform.localPosition = new Vector3(0, 0, -920);
            HudManager.Instance.ShowPopUp(Language.Language.GetString(text));
        }

        public static bool AnyShadowsBetween(Vector2 source, Vector2 dirNorm, float mag)
        {
            int num = Physics2D.RaycastNonAlloc(source, dirNorm, PhysicsHelpers.castHits, mag, Constants.ShadowMask);
            bool result = false;
            Collider2D c;
            for (int i= 0;i < num;i++)
            {
                c = PhysicsHelpers.castHits[i].collider;
                if (c.isTrigger) continue;
                if (LightSource.NoShadows.ContainsKey(c.gameObject))
                    if (LightSource.NoShadows.get_Item(c.gameObject).hitOverride == c) continue;
                if (LightSource.OneWayShadows.ContainsKey(c.gameObject))
                    if (LightSource.OneWayShadows.get_Item(c.gameObject).IsIgnored(PlayerControl.LocalPlayer.myLight)) continue;

                result = true;
                break;
            }
            return result;
        }
    }
}

