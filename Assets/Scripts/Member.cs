﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Skills;
using UnityEngine;
using UnityEngine.UI;


#endregion

namespace Assets.Scripts
{
    public class Member : IFieldObject
    {
        /// <summary>
        /// The DrawManager which is responsible for the drawing
        /// the lines and making the actions.
        /// </summary>
        public DrawManager DrawManager;

        /// <summary>
        /// A bool which indicates if this member is allowed to 
        /// draw actions.
        /// </summary>
        private bool yourTurn;

        /// <summary>
        /// A bool indicating whether the skills are done 
        /// </summary>
        private bool skillsDone;

        /// <summary>
        /// The speed of this player.
        /// The speed of the character is based on this.
        /// </summary>
        [Range(0, 250)]
        public int Speed;

        /// <summary>
        /// The stamina of this player.
        /// The length of the line is based on this.
        /// </summary>
        public int Stamina;

        /// <summary>
        /// The soundmanager which is responsible for making sounds.
        /// </summary>
        public Sound soundManager;

        /// <summary>
        /// The team where the user is in.
        /// </summary>
        public Team team;

        /// <summary>
        /// The name of the character
        /// </summary>
        public string PlayerName;

        public SkillUseIndicatorManager SkillUseIndicatorManager;

        public SkillIndicatorMember SkillIndicatorMember;


        /// <summary>
        /// The default character of this user.
        /// </summary>
        private Color defaultColor;

        public SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (team != null)
            {
                defaultColor = team.color;
            }
            else
            {
                System.Random random = new System.Random();
                float r = random.Next(1, 255) / 255f;
                float g = random.Next(1, 255) / 255f;
                float b = random.Next(1, 255) / 255f;
                Color color = new Color(r, g, b);
                this.SetColor(color);
            }

        }

        /// <summary>
        /// Returns whether the member has finished performing its action
        /// </summary>
        /// <returns>Whether their action is finished, or if the phase is in planning mode, whichever is true</returns>
        public override bool ActionDone()
        {
            if (GetComponent<KickAction>() == null) skillsDone = true;
            //Debug.Log(gameObject.name + (IsMoving ? ": \tis moving" : ": \tis not moving") + " (done: " + (skillsDone && !IsMoving) + ")");
            CheckMovement();
            return (skillsDone && !isMoving);
        }

        /// <summary>
        /// Checks if you're allowed
        /// to draw and if so, it draws a line
        /// from this member.
        /// </summary>
        private void OnMouseDown()
        {
            if (yourTurn)
            {
                DrawManager.SetMember(this);
            }
        }

        /// <summary>
        /// Sets the color of this member
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color? color = null)
        {
            if (spriteRenderer.color == color) return;
            if (color == null)
            {
                color = team.color;
            }
            spriteRenderer.color = (Color)color;
        }

        /// <summary>
        /// Performs the actions, walk action first, then the skills
        /// </summary>
        public void PerformActions()
        {
            if (this.GetComponent<WalkAction>() != null)
            {
                SendMessage(ActionConstants.OnMemberWalkString);
            }
            else
            {
                SendMessage(ActionConstants.OnMemberWalkDoneString);
            }
        }

        /// <summary>
        /// Use this to make sure the player
        /// can't give input when he's not
        /// supposed to. If the boolean parameter
        /// is false the drawmanager won't be 
        /// able to draw a line so -> make an action.
        /// </summary>
        /// <param name="yourTurn">
        /// The boolean which decides 
        /// if this member may move.
        /// </param>
        public void ChangeTurn(bool yourTurn)
        {
            ResetPoints();
            SetColor();
            this.yourTurn = yourTurn;
        }

        public void UpdateSkillIndicator(MemberLine memberLine)
        {
            SkillIndicatorMember.SetActive(memberLine.LastPosition, memberLine.GetAlmostLastPosition, this);
        }

        /// <summary>
        /// Performs an action after the member has walked
        /// </summary>
        void OnMemberWalkDone()
        {
            if (GetComponent<KickAction>() == null)
            {
                skillsDone = true;
            }
        }

        /// <summary>
        /// Performs an action after the skills have been executed
        /// </summary>
        void OnSkillExecuted()
        {
            skillsDone = true;
        }

        /// <summary>
        /// Remove all the lines of the player.
        /// Does not remove the actions.
        /// </summary>
        public void RemoveLines()
        {
            DrawManager.ClearLine(this);
        }

        /// <summary>
        /// Clear the actions of the player.
        /// This is called when a wall has
        /// been hit.
        /// </summary>
        public void WallHit()
        {
            soundManager.playBumpSound();
            WalkAction movement = gameObject.GetComponent<WalkAction>();
            if (movement != null)
            {
                Destroy(movement);
            }
        }

        /// <summary>
        /// Checks if this user is hit.
        /// If it is hit, clear it's actions
        /// and set a velocity and it's current speed.
        /// Rigidbody will take care of the rest of 
        /// the user's movement.
        /// </summary>
        /// <param name="velocity">
        /// The velocity of the player at the moment of impact.
        /// </param>
        public void IsHit(Vector3 velocity)
        {
            soundManager.playBumpSound();
            transform.GetComponent<Rigidbody2D>().velocity = velocity;
            SendMessage(ActionConstants.OnMemberWalkInterruptedString, SendMessageOptions.DontRequireReceiver);
        }

        public void ActionPressed(Type action)
        {
            if (action.IsAssignableFrom(typeof(MonoBehaviour))) throw new Exception("Type isn't monobehaviour!");
            if (GetSkillAction() != null) return;
            gameObject.AddComponent(action);
            Debug.Log("Added skill " + action.Name);
        }


        /// <summary>
        /// Set the fields in this class.
        /// Is used to import it from a file.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="stamina">The stamina of the member.</param>
        /// <param name="speed">The speed of the member.</param>
        public void SetFieldsFromFile(MemberData data)
        {
            this.PlayerName = data.Name;
            this.Stamina = data.Stamina;
            this.Speed = data.Speed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MemberData GetData()
        {
            return new MemberData()
            {
                Stamina = this.Stamina,
                Speed = this.Speed,
                Name = this.PlayerName
            };
        }

        /// <summary>
        /// Gets the type of the attached skill component
        /// </summary>
        /// <returns></returns>
        public Type GetSkill()
        {
            if (GetSkillAction() != null)
            {
                return GetSkillAction().GetType();
            }
            return null;
        }

        /// <summary>
        /// Gets the attached skill component
        /// </summary>
        /// <returns></returns>
        public Skills.Action GetSkillAction()
        {
            Skills.Action ka = GetComponent<KickAction>();
            if (ka != null) return ka;

            ka = GetComponent<TiedTogetherAction>();
            if (ka != null) return ka;

            ka = GetComponent<GrowAction>();
            if (ka != null) return ka;

            return null;
        }

        /// <summary>
        /// Removes the first skill component attached to this member
        /// </summary>
        public void RemoveSkills()
        {
            Skills.Action action = GetSkillAction();
            if (action == null)
            {
                return;
            }
            if (action is TiedTogetherAction)
            {
                if (((TiedTogetherAction)action).FirstPhase)
                {
                    Destroy(action as TiedTogetherAction);
                }

            }
            if (action is KickAction)
            {
                Destroy(action as KickAction);
            }
            if (action is GrowAction)
            {
                Destroy(action as GrowAction);
            }
        }

        /// <summary>
        /// Called from skills when they are added
        /// </summary>
        /// <param name="gm"></param>
        void OnSkillAdded(Component gm)
        {
            SkillUseIndicatorManager.SetActive(gm, true);
        }

        /// <summary>
        /// Called from skills when they are added
        /// </summary>
        /// <param name="gm"></param>
        void OnSkillRemoved(Component gm)
        {
            SkillUseIndicatorManager.SetActive(gm);
            SkillIndicatorMember.SetInactive();
        }
    }
}