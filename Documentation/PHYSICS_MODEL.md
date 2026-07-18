# PineMorph Lab Analytical Physics Model

## Scope

PineMorph Lab models a perfectly bonded active-passive bilayer under a step change in relative humidity. It is designed for comparative reasoning and parameter sensitivity, not product certification.

## Hygroscopic strain

The active layer is treated as orthotropic. Its free expansion along the actuator axis is transformed by fiber angle:

`beta(theta) = beta_L cos^2(theta) + beta_T sin^2(theta)`

`epsilon_active = beta(theta) delta_RH`

The passive layer uses a smaller isotropic hygroscopic expansion coefficient.

## Laminate equilibrium

The axial strain varies linearly through thickness:

`epsilon(z) = epsilon_0 + kappa z`

Stress in each layer is:

`sigma_i(z) = E_i [epsilon_0 + kappa z - epsilon_i_free]`

The solver enforces zero resultant axial force and zero resultant bending moment for the unloaded strip. Solving the resulting two-equation laminate system produces interface strain `epsilon_0` and curvature `kappa`.

## Geometry

For a constant-curvature hinge of length `L`:

`opening angle = |kappa| L`

`tip displacement = |(1 - cos(kappa L)) / kappa|`

## Moisture response

The 95% response estimate uses the first diffusion mode through the active-layer thickness:

`t95 = -ln(0.05) h_active^2 / (pi^2 D)`

This exposes the square-law penalty of increasing active-layer thickness.

## Instructional parameters

| Parameter | Value |
| --- | ---: |
| Total bilayer thickness | `1.6 mm` |
| Active modulus | `120 MPa` |
| Hinge length | `40 mm` |
| Longitudinal hygroexpansion | `0.015 / full RH fraction` |
| Transverse hygroexpansion | `0.120 / full RH fraction` |
| Passive hygroexpansion | `0.008 / full RH fraction` |
| Moisture diffusivity | `1.5e-9 m^2/s` |
| Humidity step | `0.55 RH fraction` |

These constants create a transparent instructional design space. Material-specific use requires measured values.

## Research basis

- Reyssat, E., and Mahadevan, L. (2009). Hygromorphs: from pine cones to biomimetic bilayers. *Journal of the Royal Society Interface, 6*(39), 951-957. https://doi.org/10.1098/rsif.2009.0184
- Dawson, C., Vincent, J. F. V., and Rocca, A. M. (1997). How pine cones open. *Nature, 390*, 668. https://doi.org/10.1038/37745

## Calibration required

- Measure longitudinal and transverse swelling over humidity cycles.
- Measure orthotropic modulus and its humidity dependence.
- Fit diffusivity from time-resolved mass uptake or curvature.
- Measure interface stress, delamination, hysteresis, and fatigue.
- Compare predicted curvature and t95 with repeated physical bilayer tests.
