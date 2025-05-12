using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ButtonPressed : MonoBehaviour
{
    private Transform buttonTransform;

    private Vector3 originalPosition;
    public Vector3 pressedPositionOffset = new Vector3(0f, -0.05f, 0f);
    public Vector3 rotation;
    public Vector3 moveAmount;
    public ButtonPressed otherButton;
    public Light light;


    public float pressDuration = 0.1f;

    public GameObject objectToMove;

    private bool pressed = false;
    public bool onlyPressedOneTime = false; //solo se puede pulsar una vez
    public bool disabled = false; //desactivar el boto
    public bool holdToActivate = false; //mantenir apretat per activar (NOMES PER AQUELLS BOTONS QUE VULGUEU QUE SI DEIXEN DE PULSAR TORNI A LA NORMALITAT)
    public bool cooperateButton = false;

    void Start()
    {
        buttonTransform = GetComponent<Transform>();
        originalPosition = buttonTransform.localPosition;
    }

    public void OnTriggerEnter(Collider other) //lo llama el GameManager
    {
        if ((other.CompareTag("Player1") || other.CompareTag("Player2")) && !pressed)
        {
            StartCoroutine(OnButtonPress());
        }
    }

    private void OnTriggerExit(Collider other) //per aquells botons que es desactivin al deixar de pulsar
    {
        if ((other.CompareTag("Player1") || other.CompareTag("Player2")) && pressed && holdToActivate)
        {
            OnButtonRelease();
        }
    }

    public IEnumerator OnButtonPress()
    {
        if (disabled) yield break;

        pressed = true; //es marca com a presionat
        buttonTransform.DOComplete(); //para l'animacio actual (si n'hi ha)
        buttonTransform.DOLocalMove(originalPosition + pressedPositionOffset, pressDuration).SetEase(Ease.OutBack); //baixa el boto

        if (objectToMove != null)
        {
            if(cooperateButton && CheckOtherButton())
            {
                if (!disabled && !otherButton.disabled) // Asegurarse de que no se activa varias veces
                {
                    SetMovmentAndRotation(); // Asigna los valores al objeto a mover
                    yield return new WaitForSeconds(0.5f);
                    objectToMove.GetComponent<Interactuable>().Interact();
                    disabled = true;
                    otherButton.disabled = true;
                }
            }
            else if(!cooperateButton)
            {
                SetMovmentAndRotation(); // Asigna los valores al objeto a mover
                yield return new WaitForSeconds(0.25f);
                objectToMove.GetComponent<Interactuable>().Interact();
            }
           
        }
        if(light != null)
        {
            light.DOIntensity(0.24f, 0.5f);
        }
        if (onlyPressedOneTime)
        {
            disabled = true;
        }
    }

    public void OnButtonRelease()
    {
        pressed = false;
        buttonTransform.DOComplete();
        buttonTransform.DOLocalMove(originalPosition, pressDuration).SetEase(Ease.OutBack);

        if (objectToMove != null && !cooperateButton)
        {
            Debug.Log("Button released");
            objectToMove.GetComponent<Interactuable>().Interact(); //crida de nou per tornar a l'estat inicial
        }
        if(light != null)
        {
            Debug.Log("Light off");
            light.DOIntensity(0f, 0.5f);
        }
    }

    public void SetMovmentAndRotation() //metode que fa un get a les variables de l'objecte que volem moure amb interactuable
    {
        Interactuable interactuable = objectToMove.GetComponent<Interactuable>();
        if (interactuable != null)
        {
            interactuable.MoveAmount = moveAmount;
            interactuable.RotationAmount = rotation;
        }
    }

    private bool CheckOtherButton()
    {
        Debug.Log($"Checking other button: {otherButton.pressed}");
        return otherButton.pressed;
    }

    /* public void OnButtonRelease()
    {
        StopAllCoroutines();  
        StartCoroutine(MoveButton(originalPosition));
    } */
}
