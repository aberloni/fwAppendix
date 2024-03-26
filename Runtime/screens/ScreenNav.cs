using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace fwp.screens
{
    /// <summary>
    /// module to manage raw input directions
    /// </summary>
    public class ScreenNav
    {
        protected float notInteractiveTimer = 0f;

        float moveTimerDelayTime = 0.1f;
        float moveTimerDelay = 0f; // can't spam movement in menu

        public Action onPressedDown;
        public Action onPressedUp;
        public Action onPressedLeft;
        public Action onPressedRight;
        public Action onBack;

        public ScreenNav()
        {
            resetTimerNoInteraction();
        }

        protected bool isDelaying()
        {
            return moveTimerDelay > 0f;
        }
        protected void resetTimerDelay()
        {
            moveTimerDelay = moveTimerDelayTime;
        }

        public void resetTimerNoInteraction()
        {
            notInteractiveTimer = 0.2f; // to kill interactive frame offset
        }

        protected void update_input_keyboard()
        {
            if (moveTimerDelay > 0f) return;

            bool any = Input.anyKey;

            //Debug.Log("input:any ? " + any);

            if(any) // key down !
            {
                
            }
            else // key up !
            {
                Vector2 dir = Vector2.zero;

                if (Input.GetKeyUp(KeyCode.UpArrow)) dir.y = 1f;
                else if (Input.GetKeyUp(KeyCode.DownArrow)) dir.y = -1f;

                if (Input.GetKeyUp(KeyCode.LeftArrow)) dir.x = -1f;
                else if (Input.GetKeyUp(KeyCode.RightArrow)) dir.x = 1f;

                pressedDirection(dir);

                if (Input.GetKeyUp(KeyCode.Escape)) onBack?.Invoke();
            }
            
        }

        void pressedDirection(Vector2 dir)
        {
            resetTimerDelay();

            if (dir.sqrMagnitude == 0f) return;

            if (dir.x > 0f) onPressedRight?.Invoke();
            else if (dir.x < 0f) onPressedLeft?.Invoke();

            if (dir.y > 0f) onPressedUp?.Invoke();
            else if (dir.y < 0f) onPressedDown?.Invoke();
        }

        public void update()
        {
            if (moveTimerDelay > 0f)
            {
                moveTimerDelay -= Time.deltaTime;
            }

            if (notInteractiveTimer > 0f)
            {
                notInteractiveTimer -= Time.deltaTime;
                return;
            }

            update_input_keyboard();
        }

        virtual protected void pressed_up() { resetTimerDelay(); if (onPressedUp != null) onPressedUp(); }
        virtual protected void pressed_down() { resetTimerDelay(); if (onPressedDown != null) onPressedDown(); }
        virtual protected void pressed_left() { resetTimerDelay(); if (onPressedLeft != null) onPressedLeft(); }
        virtual protected void pressed_right() { resetTimerDelay(); if (onPressedRight != null) onPressedRight(); }

        virtual protected bool presssed_skip() => false;

    }

}

