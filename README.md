# SocraticSwarm

A simulator and algorithms using deccentralized receding horizon control for coordinating autonomous UAV systems in completing a search task. 

## Resource References

Terrain: http://forum.unity3d.com/threads/cool-terrain-free-assets-with-tutorial.166228/

SwarmOps (PSO and DE): http://www.hvass-labs.org/projects/swarmops/

Nelder-Mead Minimization: https://www.openhub.net/p/nelder-mead-simplex

## Cost Adaptation for Robust Decentralized Swarm Behaviour

The multi-agent swarm system is a robust paradigm which can drive efficient completion of complex tasks even under energy limitations and time constraints. However, coordination of a swarm from a centralized command center can be difficult, particularly as the swarm becomes large and spans wide ranges. Here, we leverage propagation of messages based on mesh-networking protocols for global communication in the swarm and online cost-optimization through decentralized receding horizon control to drive decentralized decision-making. Our cost-based formulation allows for a wide range of tasks to be encoded. To ensure this, we implement a method for adaptation of costs and constraints which ensures effectiveness with novel tasks, network delays, heterogeneous flight capabilities, and increasingly large swarms. We use the Unity3D game engine to build a simulator capable of introducing artificial networking failures and delays in the swarm. Using the simulator we validate our method using an example coordinated exploration task. We release our simulator and code to the community for future work.

## Usage

Simply download Unity3D, import the project and press play to run simulations. Simulation parameters can be adjusted by global variables in the code.

## Citation

```
@article{henderson2017swarm,
   author = {{Henderson}, Peter and {Vertescher}, Matthew and {Meger}, David and {Coates}, Mark},
    title = "{Cost Adaptation for Robust Decentralized Swarm Behaviour}",
  journal = {arXiv preprint arXiv:1709.07114},
     year = 2017,
       url={https://arxiv.org/pdf/1709.07114.pdf}
}
```
