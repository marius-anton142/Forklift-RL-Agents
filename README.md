This project explores the training process and the robustness of a forklift agent in a simulated environment. Using Deep Reinforcement Learning, Imitation Learning, and Curriculum Learning, the agent is designed to navigate, maneuver, and accurately transport packages through a warehouse.

| ![Forklift](img/forklift01.png) |
|:--:|
| *Forklift lifting a pallet.* |

Observations and Actions: The agent gathers observations about the state of the environment and performs discrete actions such as accelerating, steering, and moving the fork. The observations include data about the distance to objects, detected through proximity sensors.

| ![Forklift](img/ray01.png) |
|:--:|
| *Large red spheres that detect the wall, medium yellow spheres that detect the checkpoint.* |

| ![Forklift](img/ray02.png) |
|:--:|
| *Sphere cast from the tips of the forks inside the pallets.* |
