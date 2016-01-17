using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NavMeshAgent))]
[RequireComponent (typeof (Animator))]
public class Zombie : MonoBehaviour {

	//玩家
	private Player player;

	//寻路代理
	private NavMeshAgent agent;

	//动画
	private Animator animator;

	public float moveSpeed = 0.5f;

	public int hp;

	private float rotateSpeed = 120;

	private float actTimer = 2;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if(player.IsDeath()){
			return;
		}
			
		if (animator.IsInTransition (0)) {
			return;
		}

		//基于状态机的判断播放相应的行为动画
		AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
		if(InState(state, "idle")){
			animator.SetBool("idle", false);

			actTimer -= Time.deltaTime;
			if(actTimer > 0){
				return;
			}

			if(DistanceToPlayerLessEnough()) {
				animator.SetBool ("attack", true);
			}else{
				animator.SetBool("run", true);
				//agent.SetDestination(player.transform.position);
				actTimer = 1;
			}
		}else if(InState(state, "run")){
			animator.SetBool("run", false);

			if(DistanceToPlayerLessEnough ()){
				animator.SetBool ("attack", true);
				agent.ResetPath ();
				return;
			}else{
				actTimer -= Time.deltaTime;
				if(actTimer < 0){
					agent.SetDestination(player.transform.position);
					actTimer = 1;
				}

				//发现其实只需要agent.SetDestination便可以自动寻路了，都不需要调用agent.Move
				//MoveToPlayer();
			}
		}else if(InState(state, "attack")){
			animator.SetBool("attack", false);

			RotateToPlayer();

			if(state.normalizedTime >= 1f){
				animator.SetBool("idle", true);
				actTimer = 2;
			}
		}
	}

	bool InState(AnimatorStateInfo state, string stateStr){
		//return state.fullPathHash == Animator.StringToHash("Base Layer."+stateStr);
		//使用如下的IsName方法替代上面的哈希值判断
		return state.IsName(stateStr);
	}

	bool DistanceToPlayerLessEnough(){
		return DistanceToPlayerLessThan(1.5f);
	}

	bool DistanceToPlayerLessThan(float distance){
		return Vector3.Distance (transform.position, player.transform.position) < distance;
	}

	/*
	void MoveToPlayer(){
		agent.SetDestination(player.transform.position);
		agent.Move(transform.TransformDirection(new Vector3(0, 0, moveSpeed*Time.deltaTime)));
	}
	*/

	void RotateToPlayer(){
		Vector3 old = transform.eulerAngles;
		transform.LookAt(player.transform);
		float playerY = transform.eulerAngles.y;

		float speed = rotateSpeed * Time.deltaTime;
		float angle = Mathf.MoveTowardsAngle(old.y, playerY, speed);
		transform.eulerAngles = new Vector3(0, angle, 0);
	}

}
