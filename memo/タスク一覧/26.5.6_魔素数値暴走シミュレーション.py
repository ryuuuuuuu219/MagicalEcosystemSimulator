#!/usr/bin/env python3
"""
Mana economy burst-check simulator.

This is intentionally independent from Unity. It ignores movement mana cost,
idle mana cost, search interval variance, and generation replacement.
"""

from __future__ import annotations

import argparse
import csv
import math
from dataclasses import dataclass
from pathlib import Path


@dataclass
class Params:
    duration: float = 600.0
    dt: float = 1.0

    grass_count: int = 100
    herbivore_count: int = 30
    predator_count: int = 10

    grass_initial_hp: float = 1.0
    herbivore_initial_hp: float = 35.0
    predator_initial_hp: float = 55.0
    grass_initial_mana: float = 30.0
    herbivore_initial_mana: float = 100.0
    predator_initial_mana: float = 200.0

    mana_absorb_from_field_per_sec: float = 0.0
    field_absorb_interval: float = 1.0
    corpse_lifetime_scale: float = 10.0
    is_convert_death_release: bool = False
    death_release_log_scale: float = 1.0

    grass_eat_amount_per_touch: float = 10.0
    grass_spawn_cooldown: float = 20.0
    eat_rate_per_sec: float = 0.5
    herbivore_eat_interval: float = 50.0

    attack_damage: float = 6.0
    attack_interval: float = 1.35
    attack_hit_rate: float = 1.0
    damage_to_mana_rate: float = 1.0
    attack_mana_cost: float = 2.1
    same_target_drain_cooldown: float = 0.0
    corpse_eat_rate_per_sec: float = 1.0
    corpse_detect_range: float = 20.0
    prey_detect_range: float = 20.0
    is_convert_attack_absorb: bool = False
    attack_absorb_log_scale: float = 1.0

    magic_cooldown: float = 0.0
    magic_hit_rate: float = 1.0
    magic_damage: float = 0.0
    magic_mana_cost: float = 0.0
    magic_damage_to_mana_rate: float = 1.0
    magic_recovery_window: float = 0.0
    magic_target_count: int = 1
    magic_max_net_gain_per_cast: float = -1.0
    phase_magic_cost_multiplier: float = 1.0
    is_convert_magic_absorb: bool = False
    magic_absorb_log_scale: float = 1.0

    field_initial_mana: float = 0.0
    field_diffusion_rate: float = 0.0
    field_dissipation_rate: float = 0.0
    field_absorb_limit_per_creature: float = -1.0
    field_hotspot_attraction_weight: float = 1.0
    field_mana_radius_on_death: float = 3.0
    is_convert_field_absorb: bool = False
    field_absorb_log_scale: float = 1.0

    phase_check_interval: float = 10.0
    phase_up_mana_coefficient: float = 0.00001
    phase_up_probability_cap: float = 0.005

    csv: Path = Path("memo/タスク一覧/26.5.6_mana_simulation_result.csv")


@dataclass
class State:
    alive_grass: int
    grass_respawn_queue: list[float]
    herbivore_hp_pool: float
    predator_hp_pool: float
    herbivore_mana: float
    predator_mana: float
    corpse_mana: float
    field_mana: float
    alive_herbivores: int
    alive_predators: int
    attack_gain: float = 0.0
    magic_gain: float = 0.0
    grass_gain: float = 0.0
    field_gain: float = 0.0
    corpse_gain: float = 0.0
    mana_spent: float = 0.0
    death_release: float = 0.0
    expected_phase_ups: float = 0.0


def convert_log(value: float, enabled: bool, scale: float) -> float:
    value = max(0.0, value)
    if not enabled:
        return value
    return math.log1p(value) * max(0.0, scale)


def corpse_max_lifetime(death_mana: float, scale: float) -> float:
    return math.sqrt(max(0.0, scale) * max(0.0, death_mana))


def run(params: Params) -> list[dict[str, float]]:
    state = State(
        alive_grass=params.grass_count,
        grass_respawn_queue=[],
        herbivore_hp_pool=params.herbivore_count * params.herbivore_initial_hp,
        predator_hp_pool=params.predator_count * params.predator_initial_hp,
        herbivore_mana=params.herbivore_count * params.herbivore_initial_mana,
        predator_mana=params.predator_count * params.predator_initial_mana,
        corpse_mana=0.0,
        field_mana=params.field_initial_mana,
        alive_herbivores=params.herbivore_count,
        alive_predators=params.predator_count,
    )

    rows: list[dict[str, float]] = []
    elapsed = 0.0
    next_attack = params.attack_interval if params.attack_interval > 0 else float("inf")
    next_magic = params.magic_cooldown if params.magic_cooldown > 0 else float("inf")
    next_field_absorb = params.field_absorb_interval
    next_eat = params.herbivore_eat_interval
    next_phase_check = params.phase_check_interval
    same_target_ready = 0.0

    while elapsed <= params.duration + 0.0001:
        state.field_mana *= max(0.0, 1.0 - params.field_dissipation_rate * params.dt)

        while state.grass_respawn_queue and state.grass_respawn_queue[0] <= elapsed:
            state.grass_respawn_queue.pop(0)
            state.alive_grass += 1

        if elapsed >= next_eat and state.alive_herbivores > 0 and state.alive_grass > 0:
            touches = min(state.alive_herbivores, state.alive_grass)
            gain_per_touch = min(
                params.grass_eat_amount_per_touch,
                params.eat_rate_per_sec * params.herbivore_eat_interval,
            )
            gain = touches * gain_per_touch
            state.herbivore_mana += gain
            state.grass_gain += gain
            state.alive_grass -= touches
            state.grass_respawn_queue.extend([elapsed + params.grass_spawn_cooldown] * touches)
            state.grass_respawn_queue.sort()
            next_eat += params.herbivore_eat_interval

        if elapsed >= next_attack and state.alive_predators > 0 and state.alive_herbivores > 0:
            attacks = min(state.alive_predators, state.alive_herbivores)
            hits = attacks * max(0.0, min(1.0, params.attack_hit_rate))
            damage = hits * params.attack_damage
            state.herbivore_hp_pool = max(0.0, state.herbivore_hp_pool - damage)
            raw_gain = damage * params.damage_to_mana_rate
            gain = convert_log(raw_gain, params.is_convert_attack_absorb, params.attack_absorb_log_scale)
            if elapsed < same_target_ready:
                gain = 0.0
            elif params.same_target_drain_cooldown > 0:
                same_target_ready = elapsed + params.same_target_drain_cooldown
            cost = attacks * params.attack_mana_cost
            state.predator_mana += gain - cost
            state.attack_gain += gain
            state.mana_spent += cost
            next_attack += params.attack_interval

        if elapsed >= next_magic and state.alive_predators > 0 and state.alive_herbivores > 0:
            casts = state.alive_predators
            targets = min(state.alive_herbivores, max(1, params.magic_target_count))
            hits = targets * max(0.0, min(1.0, params.magic_hit_rate))
            damage = casts * hits * params.magic_damage
            state.herbivore_hp_pool = max(0.0, state.herbivore_hp_pool - damage)
            raw_gain = damage * params.magic_damage_to_mana_rate
            gain = convert_log(raw_gain, params.is_convert_magic_absorb, params.magic_absorb_log_scale)
            cost = casts * params.magic_mana_cost * params.phase_magic_cost_multiplier
            if params.magic_max_net_gain_per_cast >= 0.0:
                max_total_net = casts * params.magic_max_net_gain_per_cast
                gain = min(gain, cost + max_total_net)
            state.predator_mana += gain - cost
            state.magic_gain += gain
            state.mana_spent += cost
            next_magic += params.magic_cooldown

        expected_alive_herbivores = math.ceil(state.herbivore_hp_pool / max(0.0001, params.herbivore_initial_hp))
        if expected_alive_herbivores < state.alive_herbivores:
            deaths = state.alive_herbivores - expected_alive_herbivores
            mana_per_dead = state.herbivore_mana / max(1, state.alive_herbivores)
            death_mana = deaths * mana_per_dead
            state.herbivore_mana = max(0.0, state.herbivore_mana - death_mana)
            released = convert_log(death_mana, params.is_convert_death_release, params.death_release_log_scale)
            state.corpse_mana += max(0.0, death_mana - released)
            state.field_mana += released
            state.death_release += released
            state.alive_herbivores = max(0, expected_alive_herbivores)

        if state.corpse_mana > 0.0:
            lifetime = corpse_max_lifetime(state.corpse_mana, params.corpse_lifetime_scale)
            decay = state.corpse_mana / max(0.0001, lifetime)
            released = min(state.corpse_mana, decay * params.dt)
            state.corpse_mana -= released
            state.field_mana += released
            state.death_release += released

            corpse_eaten = min(
                state.corpse_mana,
                params.corpse_eat_rate_per_sec * state.alive_predators * params.dt,
            )
            state.corpse_mana -= corpse_eaten
            state.predator_mana += corpse_eaten
            state.corpse_gain += corpse_eaten

        if elapsed >= next_field_absorb and state.field_mana > 0.0:
            creatures = state.alive_herbivores + state.alive_predators
            raw_absorb = creatures * params.mana_absorb_from_field_per_sec * params.field_hotspot_attraction_weight
            if params.field_absorb_limit_per_creature >= 0.0:
                raw_absorb = min(raw_absorb, creatures * params.field_absorb_limit_per_creature)
            absorb = min(state.field_mana, convert_log(raw_absorb, params.is_convert_field_absorb, params.field_absorb_log_scale))
            state.field_mana -= absorb
            if creatures > 0:
                predator_share = state.alive_predators / creatures
                state.predator_mana += absorb * predator_share
                state.herbivore_mana += absorb * (1.0 - predator_share)
            state.field_gain += absorb
            next_field_absorb += params.field_absorb_interval

        if elapsed >= next_phase_check:
            grid_mana = state.field_mana
            probability = min(
                params.phase_up_probability_cap,
                max(0.0, params.phase_up_mana_coefficient * grid_mana),
            )
            state.expected_phase_ups += state.alive_predators * probability
            next_phase_check += params.phase_check_interval

        state.predator_mana = max(0.0, state.predator_mana)
        state.herbivore_mana = max(0.0, state.herbivore_mana)
        total_creature_mana = state.predator_mana + state.herbivore_mana
        rows.append(
            {
                "time": elapsed,
                "alive_grass": state.alive_grass,
                "alive_herbivores": state.alive_herbivores,
                "alive_predators": state.alive_predators,
                "herbivore_hp_pool": state.herbivore_hp_pool,
                "predator_hp_pool": state.predator_hp_pool,
                "predator_mana": state.predator_mana,
                "herbivore_mana": state.herbivore_mana,
                "total_creature_mana": total_creature_mana,
                "corpse_mana": state.corpse_mana,
                "field_mana": state.field_mana,
                "attack_gain": state.attack_gain,
                "magic_gain": state.magic_gain,
                "grass_gain": state.grass_gain,
                "field_gain": state.field_gain,
                "corpse_gain": state.corpse_gain,
                "mana_spent": state.mana_spent,
                "death_release": state.death_release,
                "expected_phase_ups": state.expected_phase_ups,
            }
        )

        elapsed += params.dt

    return rows


def summarize(rows: list[dict[str, float]]) -> str:
    first = rows[0]
    last = rows[-1]
    delta = last["total_creature_mana"] - first["total_creature_mana"]
    per_sec = delta / max(0.0001, last["time"] - first["time"])
    return (
        f"start_total={first['total_creature_mana']:.2f} "
        f"end_total={last['total_creature_mana']:.2f} "
        f"delta={delta:.2f} "
        f"delta_per_sec={per_sec:.4f} "
        f"field_mana={last['field_mana']:.2f} "
        f"corpse_mana={last['corpse_mana']:.2f} "
        f"attack_gain={last['attack_gain']:.2f} "
        f"magic_gain={last['magic_gain']:.2f} "
        f"grass_gain={last['grass_gain']:.2f} "
        f"spent={last['mana_spent']:.2f} "
        f"expected_phase_ups={last['expected_phase_ups']:.4f}"
    )


def write_csv(path: Path, rows: list[dict[str, float]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=list(rows[0].keys()))
        writer.writeheader()
        writer.writerows(rows)


def add_bool_flag(parser: argparse.ArgumentParser, name: str, default: bool) -> None:
    dest = name.replace("-", "_")
    parser.add_argument(f"--{name}", dest=dest, action="store_true")
    parser.add_argument(f"--no-{name}", dest=dest, action="store_false")
    parser.set_defaults(**{dest: default})


def parse_args() -> Params:
    parser = argparse.ArgumentParser(description="Mana economy burst-check simulator.")
    defaults = Params()

    for field_name, value in defaults.__dict__.items():
        flag = "--" + field_name.replace("_", "-")
        if isinstance(value, bool):
            add_bool_flag(parser, field_name.replace("_", "-"), value)
        elif isinstance(value, Path):
            parser.add_argument(flag, type=Path, default=value)
        elif isinstance(value, int):
            parser.add_argument(flag, type=int, default=value)
        else:
            parser.add_argument(flag, type=float, default=value)

    args = parser.parse_args()
    return Params(**vars(args))


def main() -> None:
    params = parse_args()
    rows = run(params)
    print(summarize(rows))
    write_csv(params.csv, rows)
    print(f"csv={params.csv}")


if __name__ == "__main__":
    main()
