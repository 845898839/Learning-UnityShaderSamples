using UnityEngine;
using System.Collections;

// Inerface of ray casting executer, there could be many varations with compute shaders/CPU/Basic shaders.
public interface IRayCastingExecuter
{
	void Render(IRayCastingCamera camera);
}