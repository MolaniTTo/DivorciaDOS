using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using Photon.Pun;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    public bool walking = false;

    [Space]

    public Transform currentNode;
    public Transform clickedNode;
    public Transform marker;

    [Space]

    public List<Transform> finalPath = new List<Transform>();

    private float blend;

    public bool isEnabled = true;

    public bool isClimbing = false;

    public bool wasClimbing = false;

    public PhotonView photonView; //es per que funcioni el multiplayer

    Transform currentPlatform = null;
    Vector3 lastPlatformPosition;
    Quaternion lastPlatformRotation;
    bool isOnMovingGround = false;

    public AudioSource audioSource;

    void Start()
    {
        GetInfoOfCurrentNode(); //pilla la info del node actual
        
        if (currentNode.GetComponent<Navigable>().movingGround && currentNode.parent != null)
        {
            currentPlatform = currentNode.parent;
            lastPlatformPosition = currentPlatform.position;
            lastPlatformRotation = currentPlatform.rotation;
            isOnMovingGround = true;
        }
       
        marker = GameObject.FindGameObjectWithTag("indicator").transform;
        photonView = GetComponent<PhotonView>();
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 20;

        audioSource = GetComponent<AudioSource>();
        
    }


    void Update()
    {

        GetInfoOfCurrentNode(); //pilla la info del node actual (tot el rato)

        /*if (currentNode.GetComponent<Navigable>().movingGround) //si el node actual te movingGround activat (es a dir, es una plataforma que es mou) el jugador es moura amb la plataforma
        {
            Debug.Log("Moving ground");
            transform.parent = currentNode.parent;
        }
        else
        {
            transform.parent = null;
        }*/

        if (photonView.IsMine) //es per que funcioni el multiplayer i que nomes el jugador que controla el personatge pugui moure'l
        {
            if (isEnabled)
            {
                if(walking == false)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit; //tira el raycast per veure on ha clicat el jugador

                        if (Physics.Raycast(mouseRay, out mouseHit))
                        {
                            if (mouseHit.transform.GetComponent<Navigable>() != null) //si el que ha clicat es un node
                            {
                                clickedNode = mouseHit.transform;
                                DOTween.Kill(gameObject.transform); //para l'animacio actual
                                finalPath.Clear(); //neteja el path actual
                                FindPath(); //busca el path fins al node clicat

                                blend = transform.position.y - clickedNode.position.y > 0 ? -1 : 1; //si el jugador esta per sobre del node clicat, la blend sera -1, si esta per sota, sera 1 (a revisar)

                                marker.position = mouseHit.transform.GetComponent<Navigable>().GetWalkPoint(); //posa el marker al punt on ha de caminar el jugador
                                Sequence s = DOTween.Sequence(); //crea una sequencia de moviments
                                s.AppendCallback(() => marker.GetComponentInChildren<ParticleSystem>().Play()); //activa les particules del marker
                                s.Append(marker.GetComponent<Renderer>().material.DOColor(Color.white, .1f));
                                s.Append(marker.GetComponent<Renderer>().material.DOColor(Color.black, .3f).SetDelay(.2f));
                                s.Append(marker.GetComponent<Renderer>().material.DOColor(Color.clear, .3f));

                            }
                        }
                    }
                }
            }
        }
    }


   void LateUpdate()
    {
        if (isOnMovingGround && currentPlatform != null)
        {
            Vector3 delta = currentPlatform.position - lastPlatformPosition;
            transform.position += delta;
            
            Quaternion deltaRot = currentPlatform.rotation * Quaternion.Inverse(lastPlatformRotation);

            transform.RotateAround(currentPlatform.position, Vector3.up, deltaRot.eulerAngles.y);

            lastPlatformPosition = currentPlatform.position;
            lastPlatformRotation = currentPlatform.rotation;
        }
    }


    void FindPath() //busca el path fins al node clicat
    {
        List<Transform> nodesToVisit = new List<Transform>();
        List<Transform> visitedNodes = new List<Transform>();

        foreach (TransitablePath path in currentNode.GetComponent<Navigable>().possiblePaths) //per cada path possible del node actual
        {
            if (path.active) //si el path esta activat
            {
                nodesToVisit.Add(path.target); //afegeix el node al que porta el path a la llista de nodes a visitar
                path.target.GetComponent<Navigable>().previousNode = currentNode; //el node al que porta el path te com a node anterior el node actual (es guarda de quin node ve)
            }
        }

        visitedNodes.Add(currentNode); //afegeix el node actual a la llista de nodes visitats

        ExploreNode(nodesToVisit, visitedNodes); //explora els nodes a visitar
        BuildPath(); //construeix el path
    }

    void ExploreNode(List<Transform> nodesToVisit, List<Transform> visitedNodes)
    {
        Transform current = nodesToVisit.First(); //agafa el primer node de la llista de nodes a visitar
        nodesToVisit.Remove(current); //elimina el node de la llista de nodes a visitar

        if (current == clickedNode)
        {
            return;
        }

        foreach (TransitablePath path in current.GetComponent<Navigable>().possiblePaths) //per cada path possible del node actual
        {
            if (!visitedNodes.Contains(path.target) && path.active) //si no esta visitat i el path esta activat
            {
                nodesToVisit.Add(path.target); //afegeix el node al que porta el path a la llista de nodes a visitar
                path.target.GetComponent<Navigable>().previousNode = current; //el node al que porta el path te com a node anterior el node actual (es guarda de quin node ve)
            }
        }

        visitedNodes.Add(current); //afegeix el node actual a la llista de nodes visitats

        if (nodesToVisit.Any()) //si encara queden nodes a visitar
        {
            ExploreNode(nodesToVisit, visitedNodes); //explora els nodes a visitar (recursiu)
        }
    }

    void BuildPath()
    {
        Transform node = clickedNode; //comen�a pel node clicat
        while (node != currentNode) //mentre no hagi arribat al node actual
        {
            finalPath.Add(node); //afegeix el node al path final
            if (node.GetComponent<Navigable>().previousNode != null)
            {
                node = node.GetComponent<Navigable>().previousNode; //el node actual passa a ser el node anterior
            }

            else { return; } //si no te node anterior, surt
        }

        finalPath.Insert(0, clickedNode); //afegeix el node clicat al path final

        FollowPath(); //segueix el path
    }

    void FollowPath()
    {
        Sequence s = DOTween.Sequence(); //crea una sequencia de moviments
        walking = true;

        if(audioSource != null && !audioSource.isPlaying) //si el audio source no esta sonant, el fa sonar
        {
            audioSource.Play();
        }

        for (int i = finalPath.Count - 1; i > 0; i--) //per cada node del path final
        {
            Navigable nav = finalPath[i].GetComponent<Navigable>(); //agafa el component Navigable del node
            float time = nav.isStair ? 1.5f : 1; //si el node es una escala, el temps de moviment sera 1.5, sino, sera 1
            if(nav.blockPlayer) { isEnabled = true; }  
            //si el node es fill d'un interactuable i te la opcio de for�ar escalada activada, el player escalara
            if((nav.isChildrenOfInteractuable && nav.GetInclination()) || nav.forceClimbing) { isClimbing = true; }
            else { isClimbing = false; } //sino, no escalara

            if (nav.ifPlayerWalksDisable && nav.GetComponentInParent<Interactuable>().isEnabled == true) //si el node desactiva el interactuable quan el player esta assobre i el interactuable esta activat
            {
                Debug.Log("Desactiva interactuable");
                nav.DesactivateInteractuable(); //el desactiva
            }
            else if(!nav.ifPlayerWalksDisable && nav.neighbourInteractuable != null && nav.neighbourInteractuable.isEnabled == false) //si el node activa el interactuable quan el player esta assobre i el interactuable esta desactivat
            {
                Debug.Log("Activa interactuable");
                nav.ActivateInteractuable(); //l'activa
            }
            if (nav.GetComponentInParent<Interactuable>() != null && nav.GetComponentInParent<Interactuable>().isPlayerNecessary)
            {
                Interactuable interactuable = nav.GetComponentInParent<Interactuable>();
                interactuable.isPlayer = true; //marca el interactuable com a que el player hi es
            }

            //moviment del player
            s.Append(transform.DOMove(nav.GetWalkPoint(), .2f * time).SetEase(Ease.Linear)); //mou el player al punt on ha de caminar

            if (isClimbing) //si esta escalant i ja estava escalant
            {
                //Vector3 groundNormal = currentNode.up; //Obtiene la normal del nodo actual
                //transform.rotation = Quaternion.LookRotation(-nav.transform.forward, groundNormal); //rota el player per mirar a la direccio correcta
                //Vector3 upDirection = groundNormal; 
                //if (walking) { transform.rotation = Quaternion.LookRotation(nav.transform.position - transform.position, upDirection); } //si no esta caminant, rota per mirar al node
                s.Join(transform.DOLookAt(nav.transform.position, .1f, AxisConstraint.X, nav.transform.up));
            }

            else
            {
                s.Join(transform.DOLookAt(nav.transform.position, .1f, AxisConstraint.Y, Vector3.up));
            }

            /*if (nav.isCurved) //si el node es una curva, rota segons la rotacio asignada al inspector del node
            {
                s.Join(transform.DORotate(nav.customRotation, .2f).SetEase(Ease.OutSine));
            }
            else if (!nav.dontRotate) //si no es una curva i no te l'opcio de no rotar activada, rota per mirar al node
            {
                s.Join(transform.DOLookAt(nav.transform.position, .1f, AxisConstraint.None, Vector3.up));
            }*/
        }

        //ARA MATEIX HO CRIDEM DESDE EL BUTTONPRESSED
        /*if (clickedNode.GetComponent<Navigable>().isButton)
        {
            s.AppendCallback(() => GameManager.instance.ButtonPressed(clickedNode.GetComponentInChildren<ButtonPressed>()));
        }*/

        // Limpieza al terminar el movimiento
        s.AppendCallback(() =>
        {
            currentNode = clickedNode;
            Clear(); //neteja el cami final
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); //para el so
            }

        });
    }


    void Clear()
    {
        foreach (Transform t in finalPath)
        {
            t.GetComponent<Navigable>().previousNode = null; //neteja els nodes visitats
        }
        finalPath.Clear(); //neteja el path final
        walking = false; //el jugador ja no esta caminant
    }

    public void GetInfoOfCurrentNode()
    {
        Ray playerRay = new Ray(transform.GetChild(0).position, -transform.up);
        RaycastHit playerHit;

        if (Physics.Raycast(playerRay, out playerHit))
        {
            if (playerHit.transform.GetComponent<Navigable>() != null) //si el que ha tocat es un node
            {
                currentNode = playerHit.transform; //el node hitejat passa a ser el current

                Navigable nav = currentNode.GetComponent<Navigable>();

                if (nav.movingGround && currentNode.parent != null)
                {
                    if (currentPlatform != currentNode.parent)
                    {
                        currentPlatform = currentNode.parent;
                        lastPlatformPosition = currentPlatform.position;
                    }
                    isOnMovingGround = true;
                }
                else
                {
                    isOnMovingGround = false;
                    currentPlatform = null;
                }

                if (playerHit.transform.GetComponent<Navigable>().isStair) //si es escala
                {
                    DOVirtual.Float(GetBlend(), blend, .1f, SetBlend).SetTarget(this); //la blend sera la que hem definit abans
                }
                else //sino
                {
                    DOVirtual.Float(GetBlend(), 0, .1f, SetBlend).SetTarget(this); //la blend sera 0
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Ray ray = new Ray(transform.GetChild(0).position, -transform.up);
        Gizmos.DrawRay(ray);

        if (currentNode != null)
        {
            // Dibuja una l�nea desde el nodo en la direcci�n de su "up"
            Gizmos.color = Color.green;
            Gizmos.DrawRay(currentNode.position, currentNode.up * 3); // Flecha de 2 unidades

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(currentNode.position, Vector3.up * 2);
        }
        
    }

    float GetBlend()
    {
        return GetComponentInChildren<Animator>().GetFloat("Blend");
    }
    void SetBlend(float x)
    {
        if (this == null || GetComponentInChildren<Animator>() == null) return;
        GetComponentInChildren<Animator>().SetFloat("Blend", x);
    }

    public void GoToPoint(Navigable node) //funcio per fer que el player vagi a un punt concret per si sol
    {
        GetInfoOfCurrentNode(); //pilla la info del node actual
        clickedNode = node.transform;
        DOTween.Kill(gameObject.transform); //para l'animacio actual
        finalPath.Clear(); //neteja el path actual
        FindPath(); //busca el path fins al node clicat

        blend = transform.position.y - clickedNode.position.y > 0 ? -1 : 1; //si el jugador esta per sobre del node clicat, la blend sera -1, si esta per sota, sera 1 (a revisar)

    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
