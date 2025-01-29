using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DontGetFooled
{
    public class Kurth : NPC
    {
        // Yeah, Big Thinker, if you didn't know, class in classes can be private
        private class KurthHotSpot : MonoBehaviour, IClickable<int>
        {   
            public void Clicked(int playerNumber) => smiley.Jumpscare(CoreGameManager.Instance.GetPlayer(playerNumber));
            public void ClickableSighted(int player) { }
            public void ClickableUnsighted(int player) { }
            public bool ClickableHidden() => false;
            public bool ClickableRequiresNormalHeight() => true;
            public Kurth smiley;
        }


        private SpriteRenderer render;
        private SpriteRenderer renderB;
        private AudioManager audMan;
        private GameObject hotspot;
        private Image myImage;
        private int frame = -1;
        private PlayerManager pm;
        private bool decayImage = false;

        public void SayTheLine()
        {
            this.audMan.PlaySingle(BasePlugin.Asset.Get<SoundObject>("Kurth_Jumpscare"));
        }
        public override void Initialize()
        {
            base.Initialize();
            spriteRenderer[0].sprite = null;
            spriteRenderer[0].transform.position += Vector3.down * spriteRenderer[0].transform.position.y + Vector3.up * 5f;
            spriteRenderer[0].transform.localScale *= 10f;
            foreach (Collider collider in baseTrigger)
            {
                collider.enabled = false;
            }
            audMan = base.GetComponent<PropagatedAudioManager>();
            audMan.overrideSubtitleColor = false;
            navigator.SetRoomAvoidance(true);
            base.Navigator.SetSpeed(300f);
            base.Navigator.maxSpeed = 300f;
            behaviorStateMachine.ChangeState(new Kurth_FindSpot(this));
            spriteRenderer = spriteRenderer.AddToArray(new GameObject("HeldItem").AddComponent<SpriteRenderer>()); // AddComponent returns added component
            spriteRenderer[1].transform.SetParent(spriteBase.transform);
            spriteRenderer[1].gameObject.layer = spriteRenderer[0].gameObject.layer;
            spriteRenderer[1].transform.position = base.transform.position;
            spriteRenderer[1].transform.localScale = new Vector3(9.99f, 9.99f, 9.99f);
            spriteRenderer[1].material = Resources.FindObjectsOfTypeAll<Material>().Where((Material m) => m.name == "SpriteWithFog_Forward_NoBillboard").FirstOrDefault(); // Using where and FirstOrDefault is really better
            spriteRenderer[1].sprite = null;
            renderB = spriteRenderer[1];
            renderB.enabled = false;
            render = spriteRenderer[0];
            GenerateHotspot();
            hotspot.SetActive(false);
        }
        private void Hit()
        {
            this.renderB.enabled = false;
            base.transform.LookAt(pm.transform);
            GameObject original = AssetHelper.LoadAsset<GameObject>("GumOverlay");
            this.pm.gameObject.GetComponent<Entity>().AddForce(new Force(base.transform.forward, 55f, -55f));
            GameObject gameObject = Instantiate<GameObject>(original);
            Image componentInChildren = gameObject.GetComponentInChildren<Image>();
            gameObject.SetActive(base.enabled);
            gameObject.name = base.name;
            componentInChildren.sprite = null;
            componentInChildren.color = new Color(1f, 1f, 1f, 1f);
            this.myImage = componentInChildren;
            gameObject.GetComponent<Canvas>().worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(0).canvasCam;
            Destroy(gameObject, 4.1f);
            base.Invoke(nameof(Decay), 3f);
        }
        private void Decay()
        {
            this.decayImage = true;
            base.Invoke(nameof(NoDecay), 1f);
            this.behaviorStateMachine.ChangeState(new Kurth_FindSpot(this));
            this.ec.SpawnNPC(BasePlugin.Asset.Get<NPC>("Kurth"), new IntVector2(0, 0));
        }

        // Token: 0x0600000A RID: 10 RVA: 0x0000286F File Offset: 0x00000A6F
        private void NoDecay()
        {
            this.decayImage = false;
        }
        public void Jumpscare(PlayerManager player)
        {
            this.frame = -1;
            player.transform.LookAt(this.renderB.transform.position + Vector3.down * this.renderB.transform.position.y + Vector3.up * player.transform.position.y);
            this.SayTheLine();
            // Big Thinker, use nameof for aavoiding lot of problems
            this.JumpscareNextFrame();
            // Better to do this using coroutines, but I am too lazy for rewriting your code fully, read https://docs.unity3d.com/ru/2019.4/Manual/Coroutines.html for more information
            base.Invoke(nameof(JumpscareNextFrame), 0.08f);
            base.Invoke(nameof(JumpscareNextFrame), 0.16f);
            base.Invoke(nameof(JumpscareNextFrame), 0.24f);
            base.Invoke(nameof(JumpscareNextFrame), 0.32f);
            base.Invoke(nameof(JumpscareNextFrame), 0.39999998f);
            base.Invoke(nameof(JumpscareNextFrame), 0.48f);
            base.Invoke(nameof(JumpscareNextFrame), 0.56f);
            base.Invoke(nameof(JumpscareNextFrame), 0.64f);
            this.hotspot.SetActive(false);
            this.RemoveMod(player.gameObject.GetComponent<ActivityModifier>(), new MovementModifier(Vector3.zero, 0f), 0.75f);
            base.Invoke(nameof(Hit), 0.64f);
            this.pm = player;
        }
        public IEnumerator RemoveMod(ActivityModifier am, MovementModifier mm, float tim)
        {
            am.moveMods.Add(mm);
            yield return new WaitForSeconds(tim);
            am.moveMods.Remove(mm);
            yield break;
        }
        private void GenerateHotspot()
        {
            GameObject gameObject = new GameObject("Hotspot");
            gameObject.transform.parent = renderB.transform;
            gameObject.transform.position = renderB.transform.position;
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(4f, 10f, 0.05f);
            boxCollider.isTrigger = true;
            boxCollider.center = new Vector3(0f, 0f, 0f);
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.useGravity = false;
            KurthHotSpot kurthHotSpot = gameObject.AddComponent<KurthHotSpot>();
            kurthHotSpot.smiley = this;
            hotspot = gameObject;
        }
        public void Disguise(Vector3 direction)
        {
            this.renderB.sprite = BasePlugin.Asset.Get<Sprite>("KurthFakeDoor");
            this.renderB.transform.position = base.transform.position;
            this.renderB.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
            this.renderB.transform.position += direction * 4.9f;
            this.navigator.Entity.SetFrozen(true);
            this.renderB.flipX = true;
            this.renderB.flipY = false;
            this.renderB.enabled = true;
            this.renderB.color = Color.white;
            this.hotspot.SetActive(true);
            base.Invoke(nameof(ReGo), 120f);
        }
        public override void VirtualUpdate()
        {
            base.VirtualUpdate();
            if (this.decayImage & this.myImage != null)
            {
                this.myImage.color = new Color(1f, 1f, 1f, this.myImage.color.a - Time.deltaTime);
            }
        }
        private void ReGo()
        {
            bool activeInHierarchy = this.hotspot.activeInHierarchy;
            if (activeInHierarchy)
            {
                this.behaviorStateMachine.ChangeState(new Kurth_FindSpot(this));
            }
        }
        private void JumpscareNextFrame()
        {
            this.frame++;
            this.renderB.sprite = BasePlugin.Asset.Get<Sprite>("KurthAttack_" + this.frame.ToString());
        }
    }

    public class Kurth_StateBase : NpcState
    {
        public Kurth_StateBase(Kurth npc) : base(npc)
        {
            this.npc = npc;
            kurth = npc;
        }

        // Don't use new, just rename field, don't be lazy!
        //protected new Kurth npc;
        protected Kurth kurth;
    }
    public class Kurth_FindSpot : Kurth_StateBase
    {
        public Kurth_FindSpot(Kurth Kurth) : base(Kurth)
        {
        }
        public override void Enter()
        {
            base.Enter();
            this.npc.Navigator.SetSpeed(500f);
            this.npc.Navigator.maxSpeed = 500f;
            base.ChangeNavigationState(new NavigationState_DoNothing(this.npc, 0));
            this.teleport = true;
        }
        private bool CanWall(Vector3 dir)
        {
            if (Physics.Raycast(this.npc.transform.position, dir, out RaycastHit raycastHit, 5.25f, this.npc.ec.Players[0].pc.ClickLayers, QueryTriggerInteraction.Ignore))
            {
                if (raycastHit.transform.CompareTag("Wall") & (this.npc.transform.position - raycastHit.point).magnitude >= 4.75f & (this.npc.transform.position - raycastHit.point).magnitude <= 5.25f)
                {
                    this.npc.Navigator.SetSpeed(0f);
                    this.npc.Navigator.maxSpeed = 0f;
                    return true;
                }
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            if (teleport)
            {
                teleport = false;
                List<Cell> list = new List<Cell>();
                Cell[,] cells = this.npc.ec.cells;
                int upperBound = cells.GetUpperBound(0);
                int upperBound2 = cells.GetUpperBound(1);
                for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
                {
                    for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
                    {
                        Cell cell = cells[i, j];
                        if (cell.room.category == RoomCategory.Hall)
                        {
                            list.Add(cell);
                        }
                    }
                }
                Cell cell2 = list[UnityEngine.Random.Range(0, list.Count)];
                npc.transform.position = cell2.CenterWorldPosition;
                // Use foreach loop and array instead of repetitive if else, this really better
                Vector3[] directions = new Vector3[] { Vector3.forward, Vector3.right, Vector3.left, Vector3.back };
                foreach (Vector3 direction in directions)
                {
                    if (CanWall(direction))
                    {
                        kurth.Disguise(direction);
                        return;
                    }
                }
                teleport = true;
            }
        }
        private bool teleport = false;
    }
}
