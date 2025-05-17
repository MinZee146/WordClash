using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM && DNP_NewInputSystem
using UnityEngine.InputSystem;
#endif

namespace DamageNumbersPro.Demo
{
    public class DNP_2DDemo : MonoBehaviour
    {
        float nextShotTime;

        void Start()
        {
            nextShotTime = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        void Update()
        {
            HandleShooting();
        }

        void HandleShooting()
        {
            if (DNP_InputHandler.GetLeftClick())
            {
                Shoot();
                nextShotTime = Time.time + 0.3f;
            }
            else if (DNP_InputHandler.GetRightHeld() && Time.time > nextShotTime)
            {
                Shoot();
                nextShotTime = Time.time + 0.06f;
            }
        }

        void Shoot()
        {
            var mousePosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM && DNP_NewInputSystem
            if (Mouse.current != null) {
                mousePosition = Mouse.current.position.ReadValue();
            }
#else
            mousePosition = Input.mousePosition;
#endif

            //Raycast.
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = -5;
            RaycastHit hit;
            Physics.Raycast(worldPosition, Vector3.forward, out hit, 10f);

            //Select Damage Number:
            var settings = DNP_DemoManager.instance.GetSettings();
            var prefab = DNP_DemoManager.instance.GetCurrent();

            //Number:
            var number = 1 + Mathf.Pow(Random.value, 2.2f) * settings.numberRange;
            if (prefab.digitSettings.decimals == 0)
            {
                number = Mathf.Floor(number);
            }

            //Create Damage Number:
            var newDamageNumber = prefab.Spawn(worldPosition, number);

            if (hit.collider != null)
            {
                var dnpTarget = hit.collider.GetComponent<DNP_Target>();
                if(dnpTarget != null)
                {
                    dnpTarget.Hit();
                }

                newDamageNumber.SetFollowedTarget(hit.collider.transform);
            }

            //Apply Demo Settings:
            settings.Apply(newDamageNumber);
        }
    }
}
