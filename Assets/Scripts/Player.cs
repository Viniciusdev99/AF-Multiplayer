using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;


namespace PlayerUnity
{
public enum EnumPlayer
{
    Player1 = 1,
    Player2 = 2,
    Player3 = 3,
    Player4 = 4
}

public class Player : MonoBehaviour, IPunObservable
{
    NetworkController networkController;
    public GameData_SO gameData;
    public PlayerData_SO data;
    public EnumPlayer numPlayer;
    public PhotonView view;
    public Canvas myCanvas;
        public TMP_Text textNick;

        public float force = 10;
    public float torque = 10;

    public float myHealth = 10;

    Rigidbody2D rb;

    float hor;
    float ver;

    Vector2 startPos;
    Quaternion startRotation;

    public GameObject Shot;
    public GameObject ponta;

    public bool damaged,
        isMoving,
        isPlaying;

    private void Awake()
    {
    }

    private void Start()
    {

            damaged = false;
        networkController = FindObjectOfType<NetworkController>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        startRotation = transform.rotation;
        view = GetComponent<PhotonView>();
            textNick.text = view.Owner.NickName;

            if (view.IsMine)
            {
                CallSetHUD();
            }
    }

    private void Update()
    {
            myCanvas.transform.position = this.gameObject.transform.position;
            if (!view.IsMine)
            {
                return;
            }
            if (!damaged)
        {
            if(Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical"))
            {
                if(!isMoving && isPlaying)
                {
                    isPlaying = false;
                    isMoving = true;
                }
            }
            else if (Input.GetButtonUp("Horizontal") || Input.GetButtonUp("Vertical"))
            {
                isMoving = false;
                isPlaying = false;
            }

            hor = Input.GetAxis("Horizontal");
            ver = Input.GetAxis("Vertical");

                if (Input.GetButtonDown("Fire"))
                    view.RPC("Fire", RpcTarget.All);
        }
    }

    private void FixedUpdate()
    {

        if (!damaged)
        {
            Movement();
        }
    }

    public void Reset()
    {
        myHealth = 10;
        damaged = false;
        transform.rotation = startRotation;
        transform.position = networkController.GetRandomSpawnPoint().transform.position;
    }

    public void Morri(Player player)
    {
        if (this.enabled == true)
        {
            //Instancia fuma�a no modelo e n�o no centro do objeto que nao entendi onde est�
                player.AddPoints(1);
                damaged = true;
                StartCoroutine("MorreuCountdown");
        }
    }

    void Movement()
    {
        Vector2 dir = -transform.right * ver * force;
        //rb.velocity = new Vector2(dir.x, dir.y);
        rb.AddForce(dir);

        float angle = transform.rotation.eulerAngles.z;
        rb.MoveRotation(Quaternion.Euler(0, 0, angle + (-hor * torque)));

    }

    [PunRPC]
    void Fire()
    {
            var instance = Instantiate(Shot, ponta.transform.position, ponta.transform.rotation);
            instance.GetComponent<Shot>().player = this;
            instance.GetComponent<Rigidbody2D>().AddForce(-ponta.transform.right * 600);
    }

    public void AddPoints(int value)
    {
        data.score += value;
        gameData.OnUpdateHUD.Invoke();
    }

    public void SetHealth(int value, Player player)
    {
            myHealth -= value;
            if(myHealth < 1)
            {
                Morri(player);
            }
            Debug.Log(myHealth);
    }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            /*if (stream.IsWriting)
            {
                stream.SendNext(render.flipX);
            }
            else
            {
                render.flipX = (bool)stream.ReceiveNext();
            }*/
            
        }
        IEnumerator MorreuCountdown()
        {
            yield return new WaitForSeconds(5f);
            Reset();

        }
        public void CallSetHUD()
        {
            view.RPC("SetHUD", RpcTarget.All);
        }

        [PunRPC]
        public void SetHUD()
        {
            textNick.text = view.Owner.NickName;
        }
    }

}