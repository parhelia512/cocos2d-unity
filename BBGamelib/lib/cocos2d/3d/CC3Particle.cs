using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CC3ParticleController{
		public delegate void Callback(CC3ParticleController ctr);
		Callback _callback;
		ParticleSystem _particle;
		bool _running;
		public Callback callback{get{ return _callback;}set{_callback = value;}}
		public ParticleSystem particleSystem{get{return _particle;}}
		public bool running{ get { return _running; } }

		public CC3ParticleController(ParticleSystem particle){
			_particle = particle;
			_particle.Stop (true);
			_particle.gameObject.SetActive (false);
			_running = false;
		}

		public void play(){
			_running = true;
			_particle.gameObject.SetActive (true);
			_particle.Play ();
			CCDirector.sharedDirector.scheduler.schedule (this.update, this, 0, CCScheduler.kCCRepeatForever, false);
		}

		public void stop(){
			_particle.Stop ();
		}

	 	void update (float dt)
		{
			if(_particle == null || !_particle.IsAlive() || _particle.isStopped )
			{
				_running = false;
				CCDirector.sharedDirector.scheduler.unscheduleSelector (this.update, this);
				if(_callback!=null){
					_callback(this);
				}
			}
		}

	}
	public class CC3Particle : CC3Prefab
	{
		public delegate void Callback(CC3Particle spt);
		bool _isAutoDestory;
		Callback _callback;
		CC3ParticleController[] _controllers;

		public bool isAutoDestory{get{ return _isAutoDestory;}set{_isAutoDestory = value;}}
		public Callback callback{get{ return _callback;}set{_callback = value;}}

		public CC3Particle(string path) : base(path)
		{
			_isAutoDestory = true;
			_callback = null;
			NSUtils.Assert (this.particleSystems != null && this.particleSystems.Length != 0, "CC3Particle({0}): ParticleSystem not found at path {1}.", this.gameObject.activeSelf, _path);

			_controllers = new CC3ParticleController[this.particleSystems.Length];
			for (int i=0; i<_controllers.Length; i++) {
				_controllers[i] = new CC3ParticleController (this.particleSystems[i]);
				_controllers[i].callback = onControllerCallback;
			}
		}
		public CC3Particle (GameObject obj) : base(obj)
		{
			_isAutoDestory = true;
			_callback = null;
			NSUtils.Assert (this.particleSystems != null && this.particleSystems.Length != 0, "CC3Particle({0}): ParticleSystem not found at path {1}.", this.gameObject.activeSelf,  _path);

			_controllers = new CC3ParticleController[this.particleSystems.Length];
			for (int i=0; i<_controllers.Length; i++) {
				_controllers[i] = new CC3ParticleController (this.particleSystems[i]);
				_controllers[i].callback = onControllerCallback;
			}
		}

		public void play(){
			for (int i=0; i<_controllers.Length; i++) {
				_controllers[i].play();
			}
		}

		public void stop(){
			for (int i=0; i<_controllers.Length; i++) {
				_controllers[i].stop();
			}
		}

		void onControllerCallback(CC3ParticleController ctr){
			bool hasRunning = false;
			for (int i=0; i<_controllers.Length; i++) {
				if(_controllers[i].running){
					hasRunning = true;
					break;
				}
			}
			if (!hasRunning) {
				if (_callback != null) {
					_callback (this);
				}
				if (_isAutoDestory) {
					removeFromParentAndCleanup (true);
				}
			}
		}

		public override void cleanup ()
		{
			for (int i=0; i<_controllers.Length; i++) {
				_controllers[i].stop();
			}
			base.cleanup ();
		}
	}
}

