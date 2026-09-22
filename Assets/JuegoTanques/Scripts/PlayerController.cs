/**FileHeader
 * @Author: Federico Marazzi
 * @Date: 9/21/2026, 1:06:19 PM
 * @LastEditors: Federico Marazzi
 * @LastEditTime: 9/21/2026, 1:21:50 PM
 * @Description: 
 * @Copyright: Copyright (©)}) 2026 Federico Marazzi. All rights reserved.
 * @Email: federicoandresmarazzi@gmail.com
 */
using UnityEngine;

// Hereda de Entity (tiene vida, daño, muerte) y agrega movimiento + disparo del jugador
public class PlayerController : Entity
{
    [Header("Movimiento")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _rotationSpeed = 200f;

    [Header("Disparo")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _shootPoint; // punto desde donde sale la bala
    [SerializeField] private float _shootCooldown = 0.5f;

    [Header("Respawn")]
    [SerializeField] private Transform _spawnPoint; // donde reaparece al morir

    private Rigidbody _rb;
    private float _shootTimer; // cuenta el tiempo entre disparos

    protected override void Awake()
    {
        base.Awake(); // llama al Awake de Entity (inicializa vida)
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleShooting();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Input en los dos ejes
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Dirección de movimiento en el mundo 3D (ignoramos Y)
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Movemos con Rigidbody (respeta física y colliders)
        if (direction.magnitude > 0.1f)
        {
            Vector3 newPosition = _rb.position + direction * _speed * Time.fixedDeltaTime;
            _rb.MovePosition(newPosition);

            // Rotamos el tanque para que mire hacia donde se mueve
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void HandleShooting()
    {
        // Bajamos el timer cada frame
        if (_shootTimer > 0f)
            _shootTimer -= Time.deltaTime;

        // Disparo con click izquierdo o Space, si ya pasó el cooldown
        if (Input.GetButtonDown("Fire1") && _shootTimer <= 0f)
        {
            Shoot();
            _shootTimer = _shootCooldown;
        }
    }

    private void Shoot()
    {
        if (_bulletPrefab == null || _shootPoint == null) return;

        GameObject bullet = Instantiate(_bulletPrefab, _shootPoint.position, _shootPoint.rotation);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
            bc.Launch(_shootPoint.forward); // le pasamos la dirección hacia donde mira el cañón
    }

    // Override de Entity: el jugador no se destruye, hace respawn
    protected override void Die()
    {
        Debug.Log("Jugador muerto - respawneando");
        Respawn();
    }

    private void Respawn()
    {
        // Resetea vida y estado
        _isDead = false;
        _currentHealth = _maxHealth;

        // Lo manda al punto de spawn
        if (_spawnPoint != null)
            transform.position = _spawnPoint.position;
        else
            transform.position = Vector3.zero;
    }
}
